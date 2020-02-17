using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Slipways.API.Tests
{
    [ExcludeFromCodeCoverage]
    public class LogEntry
    {
        public int EventId { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}

