using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SharpestBeak.Common.Elements;

namespace SharpestBeak.Common
{
    public sealed class GameEngine : IDisposable
    {
        #region Fields

        private static readonly Random s_random = new Random();
        private static readonly TimeSpan s_stopTimeout = TimeSpan.FromSeconds(5d);

        private readonly ReaderWriterLockSlim m_syncLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly ManualResetEvent m_stopEvent = new ManualResetEvent(false);
        private bool m_disposed;
        private readonly Action<GamePaintEventArgs> m_paintCallback;
        private readonly ThreadSafeValue<ulong> m_moveCount;
        private Thread m_engineThread;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngine"/> class.
        /// </summary>
        public GameEngine(
            Action<GamePaintEventArgs> paintCallback,
            Size size,
            IEnumerable<Type> chickenLogicTypes)
        {
            #region Fields

            if (paintCallback == null)
            {
                throw new ArgumentNullException("paintCallback");
            }

            #endregion

            m_paintCallback = paintCallback;

            // Pre-initialized properties
            this.CommonData = new GameCommonData(size);
            m_moveCount = new ThreadSafeValue<ulong>(m_syncLock);

            // Post-initialized properties
            this.AllChickens = chickenLogicTypes
                .Select((item, index) => CreateChicken(item, index))
                .ToList()
                .AsReadOnly();
            this.AliveChickensDirect = new List<ChickenUnit>(this.AllChickens);
            this.AliveChickens = this.AliveChickensDirect.AsReadOnly();

            this.ShotUnitsDirect = new List<ShotUnit>();
            this.ShotUnits = this.ShotUnitsDirect.AsReadOnly();

            if (this.AllChickens.Count > this.CommonData.NominalSize.Width * this.CommonData.NominalSize.Height / 2)
            {
                throw new ArgumentException(
                    string.Format(
                        "Too many chickens ({0}) for the board of nominal size {1}x{2}.",
                        this.AllChickens.Count,
                        this.CommonData.NominalSize.Width,
                        this.CommonData.NominalSize.Height),
                    "size");
            }

            Reset();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngine"/> class.
        /// </summary>
        public GameEngine(
            Action<GamePaintEventArgs> paintCallback,
            Size size,
            params Type[] chickenTypes)
            : this(paintCallback, size, (IEnumerable<Type>)chickenTypes)
        {
            // Nothing to do
        }

        #endregion

        #region Private Methods

        private void EnsureNotDisposed()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private ChickenUnit CreateChicken(Type logicType, int index)
        {
            var logic = (ChickenUnitLogic)Activator.CreateInstance(logicType);

            var result = new ChickenUnit(logic)
            {
                UniqueIndex = index + 1
            };
            return result;
        }

        private void PositionChickens()
        {
            for (int index = 0; index < this.AllChickens.Count; index++)
            {
                var chicken = this.AllChickens[index];

                Point2D newPosition;
                do
                {
                    var discretePosition = new Point(
                        s_random.Next(this.CommonData.NominalSize.Width),
                        s_random.Next(this.CommonData.NominalSize.Height));
                    newPosition = new Point2D(
                        GameConstants.NominalCellSize * discretePosition.X + GameConstants.NominalCellSize / 2,
                        GameConstants.NominalCellSize * discretePosition.Y + GameConstants.NominalCellSize / 2);
                }
                while (this.AllChickens.Take(index).Any(
                    item => item.Position.GetDistance(newPosition) < GameConstants.NominalCellSize));

                var newAngle = (float)Math.Floor(
                    GameHelper.HalfRevolutionDegrees - s_random.NextDouble() * GameConstants.FullRevolutionAngle);

                chicken.Position = newPosition;
                chicken.BeakAngle = GameAngle.FromDegrees(GameAngle.NormalizeDegreeAngle(newAngle));
            }
        }

        private void CallPaintCallback()
        {
            m_paintCallback(new GamePaintEventArgs(GetPresentation()));
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            CallPaintCallback();
        }

        private bool IsStopping()
        {
            return m_stopEvent.WaitOne(0);
        }

        private void StartInternal()
        {
            EnsureNotDisposed();

            if (m_engineThread != null)
            {
                throw new InvalidOperationException("Engine is already running.");
            }

            Application.Idle += this.Application_Idle;

            m_engineThread = new Thread(this.DoExecuteEngine)
            {
                Name = GetType().FullName,
                IsBackground = true
            };
            this.AliveChickens.DoForEach(
                item =>
                {
                    item.Thread = new Thread(this.DoExecuteLogic)
                    {
                        IsBackground = true,
                        Name = string.Format("Unit #{0}: {1}", item.UniqueIndex, item.Logic.GetType().FullName)
                    };
                    item.Logic.Initialize();
                });
            this.ShotUnitsDirect.Clear();

            m_stopEvent.Reset();
            CallPaintCallback();
            m_engineThread.Start();

            this.AliveChickens.DoForEach(item => item.Thread.Start(item.Logic));
        }

        private void ResetInternal()
        {
            m_moveCount.Value = 0;

            this.AliveChickens
                .Select(item => item.Logic)
                .DoForEach(item => item.MoveCount = 0);

            PositionChickens();
        }

        private void DoExecuteEngine()
        {
            m_syncLock.ExecuteInWriteLock(
                () =>
                {
                    this.AliveChickens
                        .Select(item => item.Logic)
                        .DoForEach(item => item.Error = null);
                });

            var sw = new Stopwatch();
            List<EngineMoveInfo> previousMoves = null;
            while (!IsStopping())
            {
                if (m_disposed)
                {
                    return;
                }

                if (sw.IsRunning)
                {
                    while (sw.Elapsed < GameConstants.LogicPollFrequency)
                    {
                        if (IsStopping())
                        {
                            return;
                        }
                        Thread.Yield();
                    }
                }
                var timeDelta = (float)GameConstants.LogicPollFrequency.TotalSeconds;
                sw.Restart();

                m_syncLock.ExecuteInWriteLock(
                    () =>
                    {
                        var aliveChickens = this.AliveChickens
                            .Where(item => item.Logic.Error == null)
                            .ToArray();

                        var newMoves = aliveChickens
                            .Select(item => EngineMoveInfo.Create(item))
                            .Where(item => item != null)
                            .ToList();

                        var oldShotUnits = this.ShotUnitsDirect.ToArray();

                        // Processing new shot units
                        var shootingMoves = newMoves.Where(item => item.Info.FireMode != FireMode.None).ToArray();
                        shootingMoves.DoForEach(
                            item =>
                            {
                                // Is there any active shot unit from the same chicken unit?
                                if (this.ShotUnitsDirect.Any(s => s.Owner == item.Unit))
                                {
                                    if (item.Unit.ShotTimer.Elapsed < GameConstants.ShotUnit.MaximumFrequency)
                                    {
                                        DebugHelper.WriteLine("New shot from {{{0}}} has been skipped.", item.Unit);
                                        return;
                                    }
                                }

                                var shot = new ShotUnit(item.Unit);
                                this.ShotUnitsDirect.Add(shot);
                                item.Unit.ShotTimer.Restart();

                                DebugHelper.WriteLine("New shot {{{0}}} has been made by {{{1}}}.", shot, item.Unit);
                            });

                        #region Processing Shot Collisions

                        var explodedShotUnits = new List<ShotUnit>();
                        oldShotUnits.DoForEach(
                            item =>
                            {
                                item.Position = GameHelper.GetNewPosition(
                                    item.Position,
                                    item.Angle,
                                    MoveDirection.MoveForward,
                                    GameConstants.ShotUnit.DefaultSpeed,
                                    timeDelta);
                                DebugHelper.WriteLine("Shot {{{0}}} has moved.", item);

                                if (item.Position.X < -GameConstants.ShotUnit.Radius
                                    || item.Position.X > this.CommonData.RealSize.Width
                                        + GameConstants.ShotUnit.Radius
                                    || item.Position.Y < -GameConstants.ShotUnit.Radius
                                    || item.Position.Y > this.CommonData.RealSize.Height
                                        + GameConstants.ShotUnit.Radius)
                                {
                                    item.Exploded = true;
                                    explodedShotUnits.Add(item);

                                    DebugHelper.WriteLine("Shot {{{0}}} has exploded outside of game board.", item);
                                }
                            });

                        foreach (var shotUnit in oldShotUnits)
                        {
                            var shotElement = shotUnit.GetElement();

                            var injuredChicken = aliveChickens
                                .SingleOrDefault(
                                    item => !item.IsDead
                                        && CollisionDetector.CheckCollision(shotElement, item.GetElement()));
                            if (injuredChicken != null)
                            {
                                shotUnit.Exploded = true;
                                explodedShotUnits.Add(shotUnit);

                                injuredChicken.IsDead = true;
                                injuredChicken.KilledBy = shotUnit.Owner;
                                var suicide = shotUnit.Owner == injuredChicken;
                                if (!suicide)
                                {
                                    shotUnit.Owner.KillCount++;
                                }

                                DebugHelper.WriteLine(
                                    "Shot {{{0}}} has exploded and killed {{{1}}}{2}.",
                                    shotUnit,
                                    injuredChicken,
                                    suicide ? " [suicide]" : string.Empty);
                            }
                        }

                        this.ShotUnitsDirect.RemoveAll(item => explodedShotUnits.Contains(item));

                        #endregion

                        if (previousMoves != null && timeDelta > 0f)
                        {
                            foreach (var move in previousMoves)
                            {
                                if (IsStopping())
                                {
                                    return;
                                }

                                var unit = move.Unit;

                                var newPosition = GameHelper.GetNewPosition(
                                    unit.Position,
                                    unit.BeakAngle,
                                    move.Info.MoveDirection,
                                    GameConstants.ChickenUnit.DefaultRectilinearSpeed,
                                    timeDelta);
                                var newBeakAngle = GameHelper.GetNewBeakAngle(
                                    unit.BeakAngle,
                                    move.Info.BeakTurn,
                                    timeDelta);

                                // TODO: [VM] Check out-of-game-board collision

                                var newPositionElement = new ChickenElement(newPosition, newBeakAngle);
                                var otherChickens = aliveChickens.Where(item => item != unit).ToArray();
                                var conflictingChicken = otherChickens.FirstOrDefault(
                                    item => CollisionDetector.CheckCollision(newPositionElement, item.GetElement()));
                                if (conflictingChicken != null)
                                {
                                    move.Info.State = MoveInfoState.Rejected;
                                    DebugHelper.WriteLine(
                                        "Blocked collision of chicken {{{0}}} with {{{1}}}.",
                                        unit,
                                        conflictingChicken);
                                }
                                else
                                {
                                    unit.Position = newPosition;
                                    unit.BeakAngle = newBeakAngle;
                                    move.Info.State = MoveInfoState.Handled;

                                    DebugHelper.WriteLine("Chicken {{{0}}} has moved.", unit);
                                }
                            }

                            this.AliveChickensDirect.RemoveAll(item => item.IsDead);
                        }

                        previousMoves = newMoves;
                    });

                if (IsStopping())
                {
                    return;
                }

                this.MoveCount++;
            }
        }

        private void DoExecuteLogic(object logicInstance)
        {
            var logic = logicInstance as ChickenUnitLogic;
            if (logic == null)
            {
                throw new InvalidOperationException("Invalid logic passed to thread method.");
            }

            while (!IsStopping())
            {
                if (m_disposed)
                {
                    return;
                }

                try
                {
                    var currentMove = logic.CurrentMove;
                    if (currentMove != null)
                    {
                        while (currentMove.State == MoveInfoState.None)
                        {
                            Thread.Yield();
                        }
                    }

                    logic.PreviousMove = logic.CurrentMove;
                    if (logic.ClearCurrentMoveWhileMaking)
                    {
                        logic.CurrentMove = null;
                    }

                    logic.MakeMove(new GameState(this));
                }
                catch (Exception ex)
                {
                    if (ex.IsThreadAbort())
                    {
                        throw;
                    }

                    logic.Error = ex;
                    logic.Unit.IsDead = true;

                    DebugHelper.WriteLine(
                        "Chicken #{0} is now dead since logic '{1}' caused an error:{2}{3}",
                        logic.Unit.UniqueIndex,
                        logic.GetType().FullName,
                        Environment.NewLine,
                        ex.ToString());

                    return;
                }
                logic.MoveCount++;

                Thread.Yield();
            }
        }

        private GamePresentation GetPresentation()
        {
            return m_syncLock.ExecuteInReadLock(() => new GamePresentation(this));
        }

        #endregion

        #region Internal Properties

        internal List<ChickenUnit> AliveChickensDirect
        {
            get;
            private set;
        }

        internal IList<ChickenUnit> AllChickens
        {
            get;
            private set;
        }

        internal List<ShotUnit> ShotUnitsDirect
        {
            get;
            private set;
        }

        #endregion

        #region Public Properties

        public IList<ChickenUnit> AliveChickens
        {
            get;
            private set;
        }

        public IList<ShotUnit> ShotUnits
        {
            get;
            private set;
        }

        public GameCommonData CommonData
        {
            get;
            private set;
        }

        public ulong MoveCount
        {
            [DebuggerNonUserCode]
            get
            {
                return m_moveCount.Value;
            }
            [DebuggerNonUserCode]
            private set
            {
                m_moveCount.Value = value;
            }
        }

        public bool IsRunning
        {
            [DebuggerNonUserCode]
            get
            {
                return m_syncLock.ExecuteInReadLock(() => m_engineThread != null);
            }
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            m_syncLock.ExecuteInWriteLock(this.StartInternal);
        }

        public void Stop()
        {
            EnsureNotDisposed();

            var engineThread = m_syncLock.ExecuteInReadLock(() => m_engineThread);

            Application.Idle -= this.Application_Idle;

            m_syncLock.ExecuteInReadLock(
                () => this.AliveChickens.DoForEach(
                    item =>
                    {
                        item.Thread.Abort();
                        item.Thread = null;
                    }));

            m_stopEvent.Set();
            Thread.Sleep((int)(GameConstants.LogicPollFrequency.TotalMilliseconds * 5));
            m_syncLock.ExecuteInWriteLock(
                () =>
                {
                    if (!m_engineThread.Join(s_stopTimeout))
                    {
                        m_engineThread.Abort();
                        m_engineThread.Join();
                    }

                    m_engineThread = null;
                });
        }

        public void Reset()
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException("Cannot reset game engine since it is running.");
            }

            m_syncLock.ExecuteInWriteLock(this.ResetInternal);
        }

        public void CallPaint()
        {
            CallPaintCallback();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (m_disposed)
            {
                return;
            }

            m_syncLock.EnterWriteLock();
            try
            {
                m_stopEvent.DisposeSafely();
                foreach (var item in this.AllChickens)
                {
                    item.Logic.DisposeSafely();
                }
                m_moveCount.DisposeSafely();

                m_disposed = true;
            }
            finally
            {
                m_syncLock.ExitWriteLock();
            }
            m_syncLock.DisposeSafely();
        }

        #endregion
    }
}