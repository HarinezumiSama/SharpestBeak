using System;
using BenchmarkDotNet.Loggers;

namespace SharpestBeak.Benchmarks;

internal sealed class DynamicallyPrefixedLogger(ILogger logger, Func<LogKind?, string, string> getPrefix) : ILogger
{
    private readonly object _lockObject = new();

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Func<LogKind?, string, string> _getPrefix = getPrefix ?? throw new ArgumentNullException(nameof(getPrefix));

    private bool _isNewLine = true;

    public string Id => nameof(DynamicallyPrefixedLogger);

    public int Priority { get; } = logger.Priority;

    public static DynamicallyPrefixedLogger CreateTimestampPrefixedLogger(ILogger logger)
        => new(logger, (_, _) => $"[{DateTimeOffset.Now:yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'fff' UTC'zzz}]\x0020");

    public void Write(LogKind logKind, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        lock (_lockObject)
        {
            var prefix = InternalGetPrefix(logKind, text, false);
            _logger.Write(logKind, prefix + text);
            _isNewLine = false;
        }
    }

    public void WriteLine()
    {
        lock (_lockObject)
        {
            _logger.WriteLine();
            _isNewLine = true;
        }
    }

    public void WriteLine(LogKind logKind, string text)
    {
        lock (_lockObject)
        {
            var prefix = InternalGetPrefix(logKind, text, true);
            _logger.WriteLine(logKind, prefix + text);
            _isNewLine = true;
        }
    }

    public void Flush()
    {
        lock (_lockObject)
        {
            _logger.Flush();
        }
    }

    private string InternalGetPrefix(LogKind? logKind, string text, bool isWriteLine)
        => _isNewLine && (!string.IsNullOrEmpty(text) || !isWriteLine) ? _getPrefix(logKind, text) : string.Empty;
}