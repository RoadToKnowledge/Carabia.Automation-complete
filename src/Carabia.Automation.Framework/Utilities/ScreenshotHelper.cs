using OpenQA.Selenium;

namespace Carabia.Automation.Framework.Utilities;

public static class ScreenshotHelper
{
    public static string Capture(IWebDriver driver, string outputDirectory, string fileNamePrefix)
    {
        Directory.CreateDirectory(outputDirectory);
        var filePath = Path.Combine(outputDirectory, $"{SafeFileName(fileNamePrefix)}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
        ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(filePath);
        return filePath;
    }

    public static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().Concat(['(', ')', '"', '\'', ',', ' ']).ToHashSet();
        var safe = new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return safe.Length > 120 ? safe[..120] : safe;
    }
}
