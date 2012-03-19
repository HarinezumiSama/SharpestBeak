using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpestBeak.Diagnostics
{
    public sealed class AutoStopwatch : IDisposable
    {
        #region Fields

        public static readonly string FullOutputFormat = "{0}, {1} ms, {2} ticks";
        public static readonly string TimeSpanOnlyOutputFormat = "{0}";
        public static readonly string MillisecondsOnlyOutputFormat = "{1} ms";
        public static readonly string DefaultOutputFormat = TimeSpanOnlyOutputFormat;

        private readonly object m_syncLock = new object();
        private Stopwatch m_stopwatch;
        private TimeSpan? m_elapsed;
        private TextWriter m_textWriter;
        private bool m_ownsTextWriter;
        private Action<string> m_write;
        private Action<TimeSpan> m_callback;

        private string m_outputFormat;

        #endregion

        #region Constructors

        private AutoStopwatch(
            TextWriter textWriter,
            bool ownsTextWriter,
            Action<string> write,
            Action<TimeSpan> callback)
        {
            #region Argument Check

            if (textWriter != null && write != null)
            {
                throw new ArgumentException("Both text writer and write method cannot be specified.");
            }

            #endregion

            m_textWriter = textWriter;
            m_ownsTextWriter = ownsTextWriter;
            m_write = write ?? (textWriter == null ? (Action<string>)null : textWriter.Write);
            m_callback = callback;
            m_outputFormat = DefaultOutputFormat;

            m_stopwatch = new Stopwatch();
            m_stopwatch.Start();
        }

        public AutoStopwatch(Action<TimeSpan> callback = null)
            : this(null, false, null, callback)
        {
            // Nothing to do
        }

        public AutoStopwatch(TextWriter textWriter, Action<TimeSpan> callback = null)
            : this(textWriter.EnsureNotNull(), false, null, callback)
        {
            // Nothing to do
        }

        public AutoStopwatch(StringBuilder stringBuilder, Action<TimeSpan> callback = null)
            : this(new StringWriter(stringBuilder.EnsureNotNull()), true, null, callback)
        {
            // Nothing to do
        }

        public AutoStopwatch(Action<string> write, Action<TimeSpan> callback = null)
            : this(null, false, write.EnsureNotNull(), callback)
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the time elapsed.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="AutoStopwatch"/> is not stopped yet.
        /// </exception>
        public TimeSpan Elapsed
        {
            [DebuggerNonUserCode]
            get
            {
                lock (m_syncLock)
                {
                    if (!m_elapsed.HasValue)
                    {
                        throw new InvalidOperationException(string.Format("{0} is not stopped yet.", GetType().Name));
                    }

                    return m_elapsed.Value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the output format string used to write the elapsed time to a <see cref="TextWriter"/>
        ///     or <see cref="StringBuilder"/> passed to a <see cref="AutoStopwatch"/>'s constructor.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <listheader>
        ///             <term>
        ///                 Argument
        ///             </term>
        ///             <description>
        ///                 Description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 0th argument (<b>{0}</b>)
        ///             </term>
        ///             <description>
        ///                 A <see cref="TimeSpan"/> value specifying the time elapsed.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 1st argument (<b>{1}</b>)
        ///             </term>
        ///             <description>
        ///                 A <see cref="Double"/> value specifying the total milliseconds
        ///                 elapsed.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 2nd argument (<b>{2}</b>)
        ///             </term>
        ///             <description>
        ///                 An <see cref="Int64"/> value specifying the total ticks elapsed.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public string OutputFormat
        {
            [DebuggerNonUserCode]
            get
            {
                lock (m_syncLock)
                {
                    return m_outputFormat;
                }
            }
            [DebuggerNonUserCode]
            set
            {
                #region Argument Check

                if (value.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException("The value cannot be a blank string or null.", "value");
                }

                #endregion

                lock (m_syncLock)
                {
                    m_outputFormat = value;
                }
            }
        }

        #endregion

        #region Public Methods

        public void Stop()
        {
            lock (m_syncLock)
            {
                if (m_stopwatch == null)
                {
                    return;
                }

                m_stopwatch.Stop();
                var elapsed = m_stopwatch.Elapsed;
                m_elapsed = elapsed;
                m_stopwatch = null;

                if (m_callback != null)
                {
                    m_callback(elapsed);
                    m_callback = null;
                }

                if (m_write != null)
                {
                    var outputFormat = this.OutputFormat;
                    string output;
                    try
                    {
                        output = string.Format(outputFormat, elapsed, elapsed.TotalMilliseconds, elapsed.Ticks);
                    }
                    catch (Exception ex)
                    {
                        output = string.Format(" * [Invalid output format \"{0}\": {1}] *", outputFormat, ex.Message);
                    }
                    m_write(output);

                    m_write = null;
                }

                if (m_textWriter != null)
                {
                    if (m_ownsTextWriter)
                    {
                        m_textWriter.Dispose();
                    }
                    m_textWriter = null;
                }
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///     Stops the current <see cref="AutoStopwatch"/> and releases its resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}