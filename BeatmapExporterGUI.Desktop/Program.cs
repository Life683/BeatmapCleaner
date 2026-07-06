using Avalonia;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BeatmapExporterGUI.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Register the global crash catchers before doing anything else
        AppDomain.CurrentDomain.UnhandledException += LogCrash;
        TaskScheduler.UnobservedTaskException += LogTaskCrash;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    private static void LogCrash(object sender, UnhandledExceptionEventArgs e)
    {
        WriteCrashToDisk(e.ExceptionObject?.ToString() ?? "Unknown Fatal App Crash Exception");
    }

    private static void LogTaskCrash(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        WriteCrashToDisk(e.Exception?.ToString() ?? "Unknown Background Task Exception");
        e.SetObserved(); // Prevents the application from crashing if the runtime tries to escalate it
    }

    private static void WriteCrashToDisk(string content)
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string logPath = Path.Combine(desktopPath, "BeatmapCleanerCrash.txt");

            // Using a StreamWriter configured with AutoFlush ensures that the data is forced
            // straight through the buffer memory and onto your SSD/HDD hard drive instantly.
            using var sw = new StreamWriter(logPath, true);
            sw.AutoFlush = true;
            sw.WriteLine($"[{DateTime.Now}] GLOBAL FATAL CRASH INTERCEPTED:\n{content}\n\n");
        }
        catch
        {
            // Fallback fail-silent block to prevent recursive exception loops during a crash
        }
    }
}
