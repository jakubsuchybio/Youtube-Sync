using System;
using Serilog;
using Squirrel;
using Topshelf;
using Topshelf.Squirrel.Updater;
using Topshelf.Squirrel.Updater.Interfaces;

namespace Youtube_Sync
{
    public class Program
    {
        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt")
                .CreateLogger();
            
            Log.Information("##########  Starting process '{0}', V '{1}'  ##########",
                AssemblyHelper.AssemblyTitle,
                AssemblyHelper.AssemblyVersion);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            // Start Service Updater
            IUpdater selfupdater = null;
            try 
            {
                Log.Information("Updater Initialization");
                IUpdateManager updateManager = new UpdateManager(@"c:\Updates\Youtube-Sync\", AssemblyHelper.AssemblyTitle);
                selfupdater = new RepeatedTimeUpdater(updateManager).SetCheckUpdatePeriod(TimeSpan.FromSeconds(5));
                selfupdater.Start();
            } 
            catch(Exception exx)
            {
                Log.Fatal(exx, $"'{AssemblyHelper.AssemblyTitle}' is not installed via Squirrel. Install program first.");
            }

            // Start TopShelf 
            var service = new YoutubeSyncService();
            var x = new SquirreledHost(service, AssemblyHelper.CurrentAssembly, selfupdater, true, RunAS.LocalSystem);
            //HostFactory.Run(hostConfig =>
            x.ConfigureAndRun(hostConfig =>
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
