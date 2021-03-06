﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpestBeak.Model;
using SharpestBeak.UI.Properties;

namespace SharpestBeak.UI
{
    internal sealed class LogicManager
    {
        #region Fields

        private static readonly LogicManager InstanceField = new LogicManager();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicManager"/> class.
        /// </summary>
        private LogicManager()
        {
            var logicQualifiedTypes = Settings.Default.Logics.Cast<string>().ToList();

            var logicInfos = new HashSet<LogicInfo>();
            var errors = new StringBuilder();
            foreach (var logicQualifiedType in logicQualifiedTypes)
            {
                LogicInfo logicInfo;
                try
                {
                    var type = Type.GetType(logicQualifiedType, true, false);
                    if (!typeof(ChickenUnitLogic).IsAssignableFrom(type) || type.IsAbstract
                        || type.IsGenericTypeDefinition)
                    {
                        throw new ApplicationException(
                            string.Format(
                                "Type '{0}' must inherit from '{1}' and must be non-abstract and non-generic.",
                                type.AssemblyQualifiedName,
                                typeof(ChickenUnitLogic).FullName));
                    }

                    logicInfo = new LogicInfo(type);
                }
                catch (Exception ex)
                {
                    errors.AppendLine("*** " + ex.Message);
                    continue;
                }

                logicInfos.Add(logicInfo);
            }

            this.Errors = errors.ToString();
            this.Logics = logicInfos
                .OrderBy(item => item.Caption)
                .ThenBy(item => item.Type.AssemblyQualifiedName)
                .ToList()
                .AsReadOnly();
        }

        #endregion

        #region Public Properties

        public static LogicManager Instance
        {
            [DebuggerStepThrough]
            get
            {
                return InstanceField;
            }
        }

        public string Errors
        {
            get;
            private set;
        }

        public ReadOnlyCollection<LogicInfo> Logics
        {
            get;
            private set;
        }

        #endregion
    }
}