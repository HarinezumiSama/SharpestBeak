using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpestBeak.UI.WinForms.Properties;

namespace SharpestBeak.UI.WinForms
{
    internal sealed class LogicManager
    {
        #region Fields

        private static readonly LogicManager s_instance = new LogicManager();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicManager"/> class.
        /// </summary>
        private LogicManager()
        {
            var logicQualifiedTypes = Settings.Default.Logics.Cast<string>().ToList();

            var logicTypes = new List<Type>(logicQualifiedTypes.Count);
            var errors = new StringBuilder();
            foreach (var logicQualifiedType in logicQualifiedTypes)
            {
                Type type;
                try
                {
                    type = Type.GetType(logicQualifiedType, true, false);
                }
                catch (Exception ex)
                {
                    errors.AppendLine(ex.ToString());
                    continue;
                }

                logicTypes.Add(type);
            }

            this.Errors = errors.ToString();
            this.LogicTypes = logicTypes.AsReadOnly();
        }

        #endregion

        #region Public Properties

        public static LogicManager Instance
        {
            [DebuggerStepThrough]
            get { return s_instance; }
        }

        public string Errors
        {
            get;
            private set;
        }

        public IList<Type> LogicTypes
        {
            get;
            private set;
        }

        #endregion
    }
}