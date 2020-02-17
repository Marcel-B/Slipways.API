using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static Slipways.API.Tests.ExtraControllerTests;

namespace Slipways.API.Tests
{
    [ExcludeFromCodeCoverage]
    public class FakeLogger<T> : ILogger<T>, IDisposable
    {
        public IList<LogEntry> Logs { get; }

        public FakeLogger(
            IList<LogEntry> logs)
        {
            Logs = logs;
        }

        public IDisposable BeginScope<TState>(
            TState state)
        {
            return this;
        }

        public void Dispose()
        {
        }

        public bool IsEnabled(
            LogLevel logLevel)
            => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Logs.Add(new LogEntry
            {
                Exception = exception,
                EventId = eventId.Id,
                LogLevel = logLevel,
                Message = state.ToString()
            });
        }
    }
}

