using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Model
{
    public sealed class GamePositionEventArgs : EventArgs
    {
        #region Constants and Fields

        private readonly Dictionary<ChickenUnit, DirectionalPosition> _unitToPositionMap;

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

            _unitToPositionMap = new Dictionary<ChickenUnit, DirectionalPosition>();
        }

        #endregion

        #region Public Properties

        public GameEngineData Data
        {
            get;
            private set;
        }

        public ReadOnlyCollection<ChickenUnitState> UnitStates
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

            return _unitToPositionMap.TryGetValue(unitState.Unit, out position);
        }

        public DirectionalPosition GetPosition(ChickenUnitState unitState)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            return _unitToPositionMap[unitState.Unit];
        }

        public void SetPosition(ChickenUnitState unitState, DirectionalPosition position)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            _unitToPositionMap[unitState.Unit] = position;
        }

        #endregion

        #region Internal Methods

        internal void Reset()
        {
            _unitToPositionMap.Clear();
        }

        internal DirectionalPosition GetPosition(ChickenUnit unit)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            #endregion

            return _unitToPositionMap[unit];
        }

        #endregion
    }
}