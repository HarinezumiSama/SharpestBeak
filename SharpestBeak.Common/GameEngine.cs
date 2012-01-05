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

        private static readonly TimeSpan s_stopTimeout = TimeSpan.FromSeconds(10d);

        private readonly object m_syncLock = new object();
        private readonly EventWaitHandle m_stopEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private bool m_disposed;
        private readonly Action<GamePaintEventArgs> m_paintCallback;
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
            this.NominalSize = size;
            this.RealSize = new SizeF(Constants.LargeCellSize * size.Width, Constants.LargeCellSize * size.Height);
            this.IsNextTurn = true;
            this.TurnIndex = 1;

            // Post-initialized properties
            this.AllChickens = chickenLogicTypes
                .Select((item, index) => CreateChicken(item, index))
                .ToList()
                .AsReadOnly();
            this.AliveChickensDirect = new List<ChickenUnit>(this.AllChickens);
            this.AliveChickens = this.AliveChickensDirect.AsReadOnly();

            PositionChickens();
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

            var result = new ChickenUnit(logic)
            {
                UniqueIndex = index + 1,
                Thread = new Thread(this.DoExecuteLogic) { IsBackground = true }
            };

            return result;
        }

        private void PositionChickens()
        {
            if (this.AllChickens.Count > this.NominalSize.Width * this.NominalSize.Height / 2)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Too many chickens ({0}) for the board of nominal size {1}x{2}.",
                        this.AllChickens.Count,
                        this.NominalSize.Width,
                        this.NominalSize.Height));
            }

            for (int index = 0; index < this.AllChickens.Count; index++)
            {
                var chicken = this.AllChickens[index];

                Point newPosition;
                do
                {
                    newPosition = new Point(
                        s_random.Next(this.NominalSize.Width),
                        s_random.Next(this.NominalSize.Height));
                }
                while (this.AllChickens.Take(index).Any(item => item.Position == newPosition));

                chicken.Position = new PointF(
                    Constants.LargeCellSize * newPosition.X + Constants.LargeCellSize / 2,
                    Constants.LargeCellSize * newPosition.Y + Constants.LargeCellSize / 2);
                chicken.BeakAngle = (float)Math.Floor(s_random.NextDouble() * Constants.FullRotationAngle);
            }
        }

        private void FinishGame()
        {
            lock (m_syncLock)
            {
                this.IsGameFinished = true;
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

        private void DoExecuteEngine()
        {
            var sw = new Stopwatch();
            List<EngineMoveInfo> previousMoves = null;
            while (!m_stopEvent.WaitOne(0))
            {
                if (m_disposed)
                {
                    return;
                }

                if (sw.IsRunning)
                {
                    while (sw.Elapsed < Constants.LogicPollFrequency)
                    {
                        Thread.Sleep(0);
                    }
                }
                var timeDelta = (float)sw.Elapsed.TotalSeconds;  // Will be used when calculating each move
                sw.Restart();

                var newMoves = this.AliveChickens
                    .Select(item => EngineMoveInfo.Create(item))
                    .Where(item => item != null)
                    .ToList();

                // TODO: Process fire (must be performed immediately!)

                if (previousMoves != null && timeDelta > 0f)
                {
                    foreach (var move in previousMoves)
                    {
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
            }
        }

        private void DoExecuteLogic(object logicInstance)
        {
            var logic = logicInstance as ChickenUnitLogic;
            if (logic == null)
            {
                throw new InvalidOperationException("Invalid logic passed to thread method.");
            }

            while (!m_stopEvent.WaitOne(0))
            {
                if (m_disposed)
                {
                    return;
                }

                // TODO: Obtain actual game state for this unit and its logic
                logic.MakeMove(new GameState { Engine = this });

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

        #endregion

        #region Public Properties

        public IList<ChickenUnit> AliveChickens
        {
            get;
            private set;
        }

        public Size NominalSize
        {
            get;
            private set;
        }

        public SizeF RealSize
        {
            get;
            private set;
        }

        public bool IsNextTurn
        {
            get;
            private set;
        }

        public int PlayerIndex
        {
            get;
            private set;
        }

        public bool IsGameFinished
        {
            get;
            private set;
        }

        public long TurnIndex
        {
            get;
            private set;
        }

        public bool IsRunning
        {
            get
            {
                lock (m_syncLock)
                {
                    return m_engineThread != null;
                }
            }
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            lock (m_syncLock)
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

                m_stopEvent.Reset();
                CallPaintCallback();
                m_engineThread.Start();

                this.AliveChickens.DoForEach(item => item.Thread.Start(item.Logic));
            }
        }

        public void Stop()
        {
            lock (m_syncLock)
            {
                EnsureNotDisposed();

                if (m_engineThread == null)
                {
                    return;
                }

                Application.Idle -= this.Application_Idle;

                this.AliveChickens.DoForEach(item => item.Thread.Abort());

                m_stopEvent.Set();
                if (!m_engineThread.Join(s_stopTimeout))
                {
                    m_engineThread.Abort();
                    m_engineThread.Join();
                }
                m_engineThread = null;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (m_syncLock)
            {
                m_stopEvent.DisposeSafely();
                foreach (var item in this.AllChickens)
                {
                    item.Logic.Dispose();
                }

                m_disposed = true;
            }
        }

        #endregion
    }
}