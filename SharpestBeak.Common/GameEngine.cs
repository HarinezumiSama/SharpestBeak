using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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

        private ChickenUnit CreateChicken(Type item, int index)
        {
            var logic = (ChickenUnitLogic)Activator.CreateInstance(item);

            var result = new ChickenUnit(logic) { UniqueIndex = index + 1 };
            return result;
        }

        private void PositionChickens()
        {
            for (int index = 0; index < this.AllChickens.Count; index++)
            {
                var chicken = this.AllChickens[index];

                PointF newPosition;
                do
                {
                    var discretePosition = new Point(
                        s_random.Next(this.CommonData.NominalSize.Width),
                        s_random.Next(this.CommonData.NominalSize.Height));
                    newPosition = new PointF(
                        GameConstants.LargeCellSize * discretePosition.X + GameConstants.LargeCellSize / 2,
                        GameConstants.LargeCellSize * discretePosition.Y + GameConstants.LargeCellSize / 2);
                }
                while (this.AllChickens.Take(index).Any(
                    item => item.Position.GetDistance(newPosition) < GameConstants.LargeCellSize));

                chicken.Position = newPosition;
                chicken.BeakAngle = (float)Math.Floor(
                    GameHelper.HalfRevolutionDegrees - s_random.NextDouble() * GameConstants.FullRevolutionAngle);
            }
        }

        private void CallPaintCallback()
        {
            m_paintCallback(new GamePaintEventArgs(GetPresentation()));
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            // TODO: [VM] Compute FPS
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

            m_stopEvent.Reset();
            CallPaintCallback();
            m_engineThread.Start();

            this.AliveChickens.DoForEach(item => item.Thread.Start(item.Logic));
        }

        private void StopInternal()
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
                        Thread.Sleep(1);
                    }
                }
                //var timeDelta = (float)sw.Elapsed.TotalSeconds;
                var timeDelta = (float)GameConstants.LogicPollFrequency.TotalSeconds;
                sw.Restart();

                // TODO: Check logic error and report it somehow

                var newMoves = this.AliveChickens
                    .Where(item => item.Logic.Error == null)
                    .Select(item => EngineMoveInfo.Create(item))
                    .Where(item => item != null)
                    .ToList();

                newMoves
                    .Where(item => item.FireMode != FireMode.None)
                    .DoForEach(item => this.ShotUnitsDirect.Add(new ShotUnit(item.Unit)));

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
                            move.MoveDirection,
                            timeDelta);
                        var newBeakAngle = GameHelper.GetNewBeakAngle(
                            unit.BeakAngle,
                            move.BeakTurn,
                            timeDelta);

                        // TODO: [VM] Check collisions

                        unit.Position = newPosition;
                        unit.BeakAngle = newBeakAngle;
                    }

                    // TODO: Check deaths etc.
                }

                previousMoves = newMoves;

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

                // TODO: Obtain actual game state for this unit and its logic
                try
                {
                    if (logic.ClearCurrentMoveWhileMaking)
                    {
                        logic.CurrentMove = null;
                    }
                    logic.MakeMove(new GameState(this.CommonData));
                }
                catch (Exception ex)
                {
                    if (ex.IsThreadAbort())
                    {
                        throw;
                    }

                    logic.Error = ex;
                    logic.Unit.IsDead = true;
                    return;
                }
                logic.MoveCount++;

                Thread.Sleep(1);
            }
        }

        private GamePresentation GetPresentation()
        {
            // TODO: implement
            return new GamePresentation();
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
            get { return m_moveCount.Value; }
            [DebuggerNonUserCode]
            private set { m_moveCount.Value = value; }
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
            this.StopInternal();
        }

        public void Reset()
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException("Cannot reset game engine since it is running.");
            }

            m_syncLock.ExecuteInWriteLock(this.ResetInternal);
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