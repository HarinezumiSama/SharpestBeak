using System;
using System.Diagnostics;

namespace SharpestBeak.Physics
{
    public struct QuadraticEquation
    {
        #region Constants and Fields

        private float _a;
        private float _b;
        private float _c;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="QuadraticEquation"/> class.
        /// </summary>
        public QuadraticEquation(float a, float b, float c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        #endregion

        #region Public Properties

        public float A
        {
            [DebuggerStepThrough]
            get { return _a; }
            [DebuggerStepThrough]
            set { _a = value; }
        }

        public float B
        {
            [DebuggerStepThrough]
            get { return _b; }
            [DebuggerStepThrough]
            set { _b = value; }
        }

        public float C
        {
            [DebuggerStepThrough]
            get { return _c; }
            [DebuggerStepThrough]
            set { _c = value; }
        }

        #endregion

        #region Public Methods

        public float GetDiscriminant()
        {
            return _b.Sqr() - 4f * _a * _c;
        }

        public int GetRoots(out float x1, out float x2)
        {
            if (_a.IsZero())
            {
                // Linear equation case

                if (_b.IsZero())
                {
                    x1 = 0f;
                    x2 = 0f;
                    return 0;
                }

                x1 = -_c / _b;
                x2 = x1;
                return 1;
            }

            var d = GetDiscriminant();
            if (d.IsNegative())
            {
                x1 = 0f;
                x2 = 0f;
                return 0;
            }

            var ds = d.Sqrt();
            var a2 = 2f * _a;

            x1 = (-_b - ds) / a2;
            x2 = (-_b + ds) / a2;
            return 2;
        }

        public int GetRootsOrdered(out float x1, out float x2)
        {
            var result = GetRoots(out x1, out x2);
            if (result > 1)
            {
                if (x1 > x2)
                {
                    var temp = x1;
                    x1 = x2;
                    x2 = temp;
                }
            }
            return result;
        }

        #endregion
    }
}