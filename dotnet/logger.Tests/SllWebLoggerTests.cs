using System;
using Microsoft.Commerce.Tracing;
using Microsoft.Commerce.Payments.PXCommon;
using Xunit;

public class SllWebLoggerTests
{
    [Fact]
    public void TracePXServiceException_WritesMessageToConsole()
    {
        // Arrange
        var traceActivity = EventTraceActivity.Empty;
        var message = "Test message";
        var sw = new System.IO.StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);
        try
        {
            // Act
            SllWebLogger.TracePXServiceException(message, traceActivity);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        // Assert
        var output = sw.ToString().Trim();
        Assert.Contains(message, output);
    }
}
