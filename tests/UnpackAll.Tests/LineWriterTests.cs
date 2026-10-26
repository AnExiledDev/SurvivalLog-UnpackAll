using System.Globalization;
using UnpackAll.Core;
using Xunit;

namespace UnpackAll.Tests;

public class LineWriterTests
{
    private static readonly TimeSpan Wait = TimeSpan.FromSeconds(10);
    private static readonly DateTime At = new(2026, 10, 26, 11, 32, 32, 123, DateTimeKind.Utc);

    [Fact]
    public void Lines_reach_the_file_in_the_order_they_were_logged()
    {
        var file = new StringWriter();
        var writer = new LineWriter(() => file);

        writer.Add(At, "first");
        writer.Add(At, "second");
        writer.Add(At, "third");

        Assert.True(writer.WaitIdle(Wait));
        Assert.Equal(
            new[] { LineWriter.Format(At, "first"), LineWriter.Format(At, "second"), LineWriter.Format(At, "third") },
            Lines(file));
    }

    [Fact]
    public void A_file_that_could_not_be_opened_is_opened_again_for_the_next_lines()
    {
        // A log file locked for a moment must not silence the log for the rest of the session.
        var file = new StringWriter();
        var opens = 0;
        var writer = new LineWriter(() => ++opens == 1 ? throw new IOException("locked") : file);

        writer.Add(At, "lost");
        Assert.True(writer.WaitIdle(Wait));
        writer.Add(At, "kept");
        Assert.True(writer.WaitIdle(Wait));

        Assert.Equal(new[] { LineWriter.Format(At, "kept") }, Lines(file));
    }

    [Fact]
    public void A_line_is_stamped_with_the_local_time_it_happened()
    {
        var expected = At.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CurrentCulture) + " press: done";

        Assert.Equal(expected, LineWriter.Format(At, "press: done"));
    }

    private static string[] Lines(StringWriter file) =>
        file.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
}
