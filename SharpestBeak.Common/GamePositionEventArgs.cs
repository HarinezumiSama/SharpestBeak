using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpestBeak.Common
{
    public sealed class GamePositionEventArgs : EventArgs
    {
        #region Fields

        private readonly Dictionary<ChickenUnit, DirectionalPosition> m_unitToPositionMap;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePositionEventArgs"/> class.
        /// </summary>
        internal GamePositionEventArgs(GameEngine engine)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            #endregion

            this.Data = engine.Data;

            var unitStatesProxy = new List<ChickenUnitState>(engine.AllChickens.Count);
            this.UnitStates = unitStatesProxy.AsReadOnly();
            foreach (var logic in engine.Logics)
            {
                lock (logic.UnitsStatesLock)
                {
                    unitStatesProxy.AddRange(logic.UnitsStates.Values);
                }
            }

            m_unitToPositionMap = new Dictionary<ChickenUnit, DirectionalPosition>();
        }

        #endregion

        #region Internal Methods

        internal void Reset()
        {
            m_unitToPositionMap.Clear();
        }

        internal DirectionalPosition GetPosition(ChickenUnit unit)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            #endregion

            return m_unitToPositionMap[unit];
        }

        #endregion

        #region Public Properties

        public GameEngineData Data
        {
            get;
            private set;
        }

        public IList<ChickenUnitState> UnitStates
        {
            get;
            private set;
        }

        public bool Handled
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        public bool TryGetPosition(ChickenUnitState unitState, out DirectionalPosition position)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            return m_unitToPositionMap.TryGetValue(unitState.Unit, out position);
        }

        public DirectionalPosition GetPosition(ChickenUnitState unitState)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            return m_unitToPositionMap[unitState.Unit];
        }

        public void SetPosition(ChickenUnitState unitState, DirectionalPosition position)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            m_unitToPositionMap[unitState.Unit] = position;
        }

        #endregion
    }
}