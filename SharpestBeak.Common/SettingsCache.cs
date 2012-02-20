﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpestBeak.Common.Properties;

namespace SharpestBeak.Common
{
    internal sealed class SettingsCache
    {
        #region Fields

        private static readonly SettingsCache s_instance = new SettingsCache();

        #endregion

        #region Constructors

        private SettingsCache()
        {
            ReadSettings();
        }

        #endregion

        #region Private Methods

        private void ReadSettings()
        {
            this.UsePerformanceCounters = Settings.Default.UsePerformanceCounters;
            this.EnableDebugOutput = Settings.Default.EnableDebugOutput;
            this.InstrumentationMode = Settings.Default.InstrumentationMode;
            this.DebugModeDisableShooting = Settings.Default.DebugModeDisableShooting;
        }

        #endregion

        #region Public Properties

        public static SettingsCache Instance
        {
            [DebuggerStepThrough]
            get
            {
                return s_instance;
            }
        }

        public bool UsePerformanceCounters
        {
            get;
            private set;
        }

        public bool EnableDebugOutput
        {
            get;
            private set;
        }

        public bool InstrumentationMode
        {
            get;
            private set;
        }

        // TODO: [VM] Allow to disable shooting in UI via editable setting

        public bool DebugModeDisableShooting
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public void Reload()
        {
            ReadSettings();
        }

        #endregion
    }
}