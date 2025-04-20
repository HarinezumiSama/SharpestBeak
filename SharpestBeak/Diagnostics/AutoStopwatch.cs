using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SharpestBeak.Diagnostics;

public sealed class AutoStopwatch : IDisposable
{
    public static readonly string FullOutputFormat = "{0}, {1} ms, {2} ticks";
    public static readonly string TimeSpanOnlyOutputFormat = "{0}";
    public static readonly string MillisecondsOnlyOutputFormat = "{1} ms";
    public static readonly string DefaultOutputFormat = TimeSpanOnlyOutputFormat;

    private readonly object _syncLock = new();
    private readonly bool _ownsTextWriter;
    private Stopwatch _stopwatch;
    private TimeSpan? _elapsed;
    private TextWriter _textWriter;
    private Action<string> _write;
    private Action<TimeSpan> _callback;

    private string _outputFormat;

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

    private AutoStopwatch(
        TextWriter textWriter,
        bool ownsTextWriter,
        Action<string> write,
        Action<TimeSpan> callback)
    {
        if (textWriter != null && write != null)
        {
            throw new ArgumentException("Both text writer and write method cannot be specified.");
        }

        _textWriter = textWriter;
        _ownsTextWriter = ownsTextWriter;
        _write = write ?? (textWriter is null ? null : textWriter.Write);
        _callback = callback;
        _outputFormat = DefaultOutputFormat;

        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

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
            lock (_syncLock)
            {
                if (!_elapsed.HasValue)
                {
                    throw new InvalidOperationException($"{GetType().Name} is not stopped yet.");
                }

                return _elapsed.Value;
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
    ///                 0th argument (<c>{0}</c>)
    ///             </term>
    ///             <description>
    ///                 A <see cref="TimeSpan"/> value specifying the time elapsed.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>
    ///                 1st argument (<c>{1}</c>)
    ///             </term>
    ///             <description>
    ///                 A <see cref="Double"/> value specifying the total milliseconds
    ///                 elapsed.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>
    ///                 2nd argument (<c>{2}</c>)
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
            lock (_syncLock)
            {
                return _outputFormat;
            }
        }

        [DebuggerNonUserCode]
        set
        {
            if (value.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("The value cannot be a blank string or null.", nameof(value));
            }

            lock (_syncLock)
            {
                _outputFormat = value;
            }
        }
    }

    public void Stop()
    {
        lock (_syncLock)
        {
            if (_stopwatch is null)
            {
                return;
            }

            _stopwatch.Stop();
            var elapsed = _stopwatch.Elapsed;
            _elapsed = elapsed;
            _stopwatch = null;

            if (_callback != null)
            {
                _callback(elapsed);
                _callback = null;
            }

            if (_write != null)
            {
                var outputFormat = OutputFormat;
                string output;
                try
                {
                    output = string.Format(outputFormat, elapsed, elapsed.TotalMilliseconds, elapsed.Ticks);
                }
                catch (Exception ex)
                {
                    output = $" * [Invalid output format \"{outputFormat}\": {ex.Message}] *";
                }

                _write(output);

                _write = null;
            }

            if (_textWriter != null)
            {
                if (_ownsTextWriter)
                {
                    _textWriter.Dispose();
                }

                _textWriter = null;
            }
        }
    }

    /// <summary>
    ///     Stops the current <see cref="AutoStopwatch"/> and releases its resources.
    /// </summary>
    public void Dispose() => Stop();
}