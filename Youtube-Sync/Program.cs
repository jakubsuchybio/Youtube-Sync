using System;
using Serilog;
using Topshelf;

namespace Youtube_Sync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt")
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            HostFactory.Run(windowsService =>
            {
                windowsService.Service<YoutubeSyncService>(s =>
                {
                    s.ConstructUsing(service => new YoutubeSyncService());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });

                windowsService.UseSerilog();
                windowsService.RunAsLocalSystem();
                windowsService.StartAutomatically();

                windowsService.SetDescription("This service synchronizes youtube play-lists with filesystem");
                windowsService.SetDisplayName("rEv-soft Youtube Sync");
                windowsService.SetServiceName("rEv-soft Youtube Sync");
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
