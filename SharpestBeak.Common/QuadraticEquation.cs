using System;
using System.Diagnostics;

namespace SharpestBeak.Common
{
    public struct QuadraticEquation
    {
        #region Fields

        private float m_a;
        private float m_b;
        private float m_c;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="QuadraticEquation"/> class.
        /// </summary>
        public QuadraticEquation(float a, float b, float c)
        {
            m_a = a;
            m_b = b;
            m_c = c;
        }

        #endregion

        #region Public Properties

        public float A
        {
            [DebuggerStepThrough]
            get { return m_a; }
            [DebuggerStepThrough]
            set { m_a = value; }
        }

        public float B
        {
            [DebuggerStepThrough]
            get { return m_b; }
            [DebuggerStepThrough]
            set { m_b = value; }
        }

        public float C
        {
            [DebuggerStepThrough]
            get { return m_c; }
            [DebuggerStepThrough]
            set { m_c = value; }
        }

        #endregion

        #region Public Methods

        public float GetDiscriminant()
        {
            return m_b.Sqr() - 4f * m_a * m_c;
        }

        public int GetRoots(out float x1, out float x2)
        {
            if (m_a.IsZero())
            {
                // Linear equation case

                if (m_b.IsZero())
                {
                    x1 = 0f;
                    x2 = 0f;
                    return 0;
                }

                x1 = -m_c / m_b;
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
            var a2 = 2f * m_a;

            x1 = (-m_b - ds) / a2;
            x2 = (-m_b + ds) / a2;
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