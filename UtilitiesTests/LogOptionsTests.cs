using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ptr727.Utilities.Tests;

/// <summary>
/// Serializes tests that mutate the process-global <see cref="LogOptions"/> state.
/// </summary>
[CollectionDefinition("LogOptions", DisableParallelization = true)]
public sealed class LogOptionsCollectionDefinition { }

[Collection("LogOptions")]
public sealed class LogOptionsTests
{
    [Fact]
    public void CreateLogger_UsesFactory_WhenFactorySet()
    {
        ILoggerFactory originalFactory = LogOptions.LoggerFactory;
        using TestLoggerFactory testFactory = new();

        try
        {
            LogOptions.LoggerFactory = testFactory;

            ILogger logger = LogOptions.CreateLogger("category");

            _ = LogOptions.LoggerFactory.Should().BeSameAs(testFactory);
            _ = testFactory.LastCategory.Should().Be("category");
            _ = logger.Should().NotBeNull();
        }
        finally
        {
            LogOptions.LoggerFactory = originalFactory;
        }
    }

    [Fact]
    public void CreateLogger_UsesNullLogger_WhenFactoryDefault()
    {
        ILoggerFactory originalFactory = LogOptions.LoggerFactory;

        try
        {
            LogOptions.LoggerFactory = NullLoggerFactory.Instance;

            ILogger logger = LogOptions.CreateLogger("category");

            _ = logger.Should().BeSameAs(NullLogger.Instance);
        }
        finally
        {
            LogOptions.LoggerFactory = originalFactory;
        }
    }

    [Fact]
    public void CreateLoggerGeneric_UsesTypeFullNameAsCategory()
    {
        ILoggerFactory originalFactory = LogOptions.LoggerFactory;
        using TestLoggerFactory testFactory = new();

        try
        {
            LogOptions.LoggerFactory = testFactory;

            ILogger logger = LogOptions.CreateLogger<LogOptionsTests>();

            _ = testFactory.LastCategory.Should().Be(typeof(LogOptionsTests).FullName);
            _ = logger.Should().NotBeNull();
        }
        finally
        {
            LogOptions.LoggerFactory = originalFactory;
        }
    }

    [Fact]
    public void CreateLogger_WithNullCategory_ThrowsArgumentNullException() =>
        _ = FluentActions
            .Invoking(() => LogOptions.CreateLogger(null!))
            .Should()
            .Throw<ArgumentNullException>();

    [Fact]
    public void CreateLogger_WithEmptyCategory_ThrowsArgumentException() =>
        _ = FluentActions
            .Invoking(() => LogOptions.CreateLogger(" "))
            .Should()
            .Throw<ArgumentException>();

    [Fact]
    public void SetFactory_WithNull_ResetsToNullLoggerFactory()
    {
        ILoggerFactory originalFactory = LogOptions.LoggerFactory;
        using TestLoggerFactory testFactory = new();

        try
        {
            LogOptions.SetFactory(testFactory);
            LogOptions.SetFactory(null!);

            _ = LogOptions.LoggerFactory.Should().BeSameAs(NullLoggerFactory.Instance);
        }
        finally
        {
            LogOptions.LoggerFactory = originalFactory;
        }
    }

    [Fact]
    public void TrySetFactory_WhenUnset_ReturnsTrueAndSets()
    {
        ILoggerFactory originalFactory = LogOptions.LoggerFactory;
        using TestLoggerFactory testFactory = new();

        try
        {
            LogOptions.LoggerFactory = NullLoggerFactory.Instance;

            bool result = LogOptions.TrySetFactory(testFactory);

            _ = result.Should().BeTrue();
            _ = LogOptions.LoggerFactory.Should().BeSameAs(testFactory);
        }
        finally
        {
            LogOptions.LoggerFactory = originalFactory;
        }
    }

    [Fact]
    public void TrySetFactory_WhenAlreadySet_ReturnsFalseAndDoesNotOverwrite()
    {
        ILoggerFactory originalFactory = LogOptions.LoggerFactory;
        using TestLoggerFactory testFactory = new();
        using TestLoggerFactory otherFactory = new();

        try
        {
            LogOptions.LoggerFactory = testFactory;

            bool result = LogOptions.TrySetFactory(otherFactory);

            _ = result.Should().BeFalse();
            _ = LogOptions.LoggerFactory.Should().BeSameAs(testFactory);
        }
        finally
        {
            LogOptions.LoggerFactory = originalFactory;
        }
    }

    private sealed class TestLoggerFactory : ILoggerFactory
    {
        public ILogger Logger { get; } = new TestLogger();

        public string? LastCategory { get; private set; }

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName)
        {
            LastCategory = categoryName;
            return Logger;
        }

        public void Dispose() { }
    }

    private sealed class TestLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) { }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose() { }
        }
    }
}
