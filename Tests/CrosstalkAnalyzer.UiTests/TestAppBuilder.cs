using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using CrosstalkAnalyzer;

[assembly: AvaloniaTestApplication(typeof(CrosstalkAnalyzer.UiTests.TestAppBuilder))]

namespace CrosstalkAnalyzer.UiTests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false,
            })
            .UseSkia()
            .WithInterFont();
}
