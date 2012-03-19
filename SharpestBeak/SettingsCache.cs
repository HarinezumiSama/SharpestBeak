using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpestBeak.Properties;

namespace SharpestBeak
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
            var settings = Settings.Default.EnsureNotNull();

            this.UsePerformanceCounters = settings.UsePerformanceCounters;
            this.EnableDebugOutput = settings.EnableDebugOutput;
            this.InstrumentationMode = settings.InstrumentationMode;
            this.DebugModeDisableShooting = settings.DebugModeDisableShooting;
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