using Microsoft.Extensions.Logging;
using NSubstitute;

namespace codecrafters_git.tests.TestHelpers;

public static class LoggerExtensions
{
    public static void ReceivedLogContaining<T>(this ILogger<T> logger, LogLevel level,
        params string[] expectedSubstrings)
    {
        logger.Received(1).Log(
            level,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => expectedSubstrings.All(s => v.ToString()!.Contains(s))),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    public static void ReceivedLogContaining<T>(this ILogger<T> logger, LogLevel level, string expectedSubstring)
    {
        logger.ReceivedLogContaining(level, [expectedSubstring]);
    }
}