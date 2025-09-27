using McpNetDll.Helpers;

namespace McpNetDll.Tests.Helpers;

public class PathHelperTests
{
    [Fact]
    public void ConvertWslPath_WithRegularWindowsPath_ReturnsUnchanged()
    {
        var path = @"C:\Users\Test\file.dll";

        var result = PathHelper.ConvertWslPath(path);

        Assert.Equal(path, result);
    }

    [Fact]
    public void ConvertWslPath_WithRegularLinuxPath_ReturnsUnchanged()
    {
        var path = "/home/user/file.dll";

        var result = PathHelper.ConvertWslPath(path);

        Assert.Equal(path, result);
    }

    [SkippableTheory]
    [InlineData("/mnt/c/Users/Test/file.dll", @"C:\Users\Test\file.dll")]
    [InlineData("/mnt/d/Projects/app.dll", @"D:\Projects\app.dll")]
    [InlineData("/mnt/e/data/test.dll", @"E:\data\test.dll")]
    public void ConvertWslPath_OnWindows_WithWslPath_ConvertsToWindowsPath(string input, string expected)
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "This test only runs on Windows");

        var result = PathHelper.ConvertWslPath(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertWslPath_OnNonWindows_WithWslPath_ReturnsUnchanged()
    {
        if (OperatingSystem.IsWindows())
            return; // Skip on Windows

        var paths = new[] { "/mnt/c/Users/Test/file.dll", "/mnt/d/Projects/app.dll" };

        foreach (var path in paths)
        {
            var result = PathHelper.ConvertWslPath(path);
            Assert.Equal(path, result);
        }
    }

    [Fact]
    public void ConvertWslPath_WithShortPath_ReturnsUnchanged()
    {
        var path = "/mnt/";

        var result = PathHelper.ConvertWslPath(path);

        Assert.Equal(path, result);
    }

    [Fact]
    public void ConvertWslPath_WithEmptyString_ReturnsEmpty()
    {
        var result = PathHelper.ConvertWslPath("");

        Assert.Equal("", result);
    }

    [SkippableFact]
    public void ConvertWslPath_OnWindows_WithComplexWslPath_ConvertsCorrectly()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "This test only runs on Windows");

        var input = "/mnt/c/Program Files/MyApp/bin/Debug/app.dll";
        var expected = @"C:\Program Files\MyApp\bin\Debug\app.dll";

        var result = PathHelper.ConvertWslPath(input);

        Assert.Equal(expected, result);
    }

    [SkippableFact]
    public void ConvertWslPath_OnWindows_WithLowercaseDriveLetter_ConvertsToUppercase()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "This test only runs on Windows");

        var input = "/mnt/c/test.dll";

        var result = PathHelper.ConvertWslPath(input);

        Assert.StartsWith("C:", result);
    }

    [Fact]
    public void ConvertWslPath_WithSimilarButNotWslPath_ReturnsUnchanged()
    {
        var paths = new[]
        {
            "/mnt",
            "/mntc/test",
            "mnt/c/test",
            "/mount/c/test"
        };

        foreach (var path in paths)
        {
            var result = PathHelper.ConvertWslPath(path);
            Assert.Equal(path, result);
        }
    }
}

public sealed class SkippableTheoryAttribute : TheoryAttribute
{
    public SkippableTheoryAttribute()
    {
        Skip = null;
    }
}

public sealed class SkippableFactAttribute : FactAttribute
{
    public SkippableFactAttribute()
    {
        Skip = null;
    }
}

public static class Skip
{
    public static void If(bool condition, string reason)
    {
        if (condition)
            throw new SkipException(reason);
    }

    public static void IfNot(bool condition, string reason)
    {
        if (!condition)
            throw new SkipException(reason);
    }
}

public class SkipException : Exception
{
    public SkipException(string reason) : base(reason)
    {
    }
}