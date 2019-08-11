using System;
using System.Diagnostics;
using System.Reflection;
using Serilog;
using Topshelf;
using Squirrel;
using System.Threading.Tasks;
using NuGet;

[assembly: AssemblyMetadata("SquirrelAwareVersion", "1")]

namespace Youtube_Sync
{
    public class Program
    {
        private const string _updateUrl = "http://jakubsuchy.com/Youtube-Sync/";
        private const string LOGGER_TEMPLATE = "{Timestamp:HH:mm:ss.fffffff zzz} [{Level:u3}] [{ThreadId}] {Message:j}{NewLine}{Exception}";

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: LOGGER_TEMPLATE)
                .WriteTo.File("C:/ProgramData/youtube-sync.txt", outputTemplate: LOGGER_TEMPLATE)
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            SquirrelAwareApp.HandleEvents(
                onInitialInstall: v => CallCustomInstallScript(),
                onAppUpdate: v => { },
                onAppUninstall: v => { },
                onFirstRun: () => { });

            Task.Factory.StartNew(CheckForUpdates, TaskCreationOptions.LongRunning);

            Log.Information("##########  Starting process '{0}', V '{1}'  ##########",
                AssemblyHelper.AssemblyTitle,
                AssemblyHelper.AssemblyVersion);

            // Start TopShelf 
            HostFactory.Run(hostConfig =>
            {
                hostConfig.Service<YoutubeSyncService>(s =>
                {
                    s.ConstructUsing(name => new YoutubeSyncService());
                    s.WhenStarted(ys => ys.Start());
                    s.WhenStopped(ys => ys.Stop());
                });

                hostConfig.EnableServiceRecovery(rc => rc.RestartService(1));
                hostConfig.EnableSessionChanged();
                hostConfig.UseSerilog();
                hostConfig.RunAsLocalSystem();
                hostConfig.StartAutomatically();

                hostConfig.SetDescription("This service synchronizes youtube play-lists with filesystem");
                hostConfig.SetDisplayName("rEv-soft Youtube Sync");
                hostConfig.SetServiceName("rEv-soft Youtube Sync");
            });
        }

        private static async Task CheckForUpdates()
        {
            using (var mgr = new UpdateManager(_updateUrl))
            {
                while (true)
                {
                    var currentVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                    try
                    {
                        // Check for update
                        var update = await mgr.CheckForUpdate();
                        try
                        {
                            var oldVersion = update.CurrentlyInstalledVersion?.Version ??
                                             new SemanticVersion(0, 0, 0, 0);
                            Log.Information("Installed version: {0}", oldVersion);
                            var newVersion = update.FutureReleaseEntry.Version;
                            if (oldVersion < newVersion)
                            {
                                Log.Information("Found a new version: {0}", newVersion);

                                // Download Release
                                await mgr.DownloadReleases(update.ReleasesToApply);

                                // Apply Release
                                await mgr.ApplyReleases(update);

                                CallCustomUpdateScript();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Fatal(ex, $"Error on update {currentVersion}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Fatal(ex, $"Error on check for update {currentVersion}");
                    }

                    await Task.Delay(5000);
                }
            }
        }

        private static void CallCustomInstallScript()
        {
            Debugger.Launch();
        }

        private static void CallCustomUpdateScript()
        {
            Debugger.Launch();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var obj = e.ExceptionObject;

            if (obj is Exception ex)
                Log.Fatal(ex, "Unhandled Exception");
            else if (obj != null)
                Log.Fatal($"Unknown error from CurrentDomain_UnhandledException. e.ExceptionObject: {obj}");
            else
                Log.Fatal("Unknown error from CurrentDomain_UnhandledException. e.ExceptionObject is null");
        }
    }
}
