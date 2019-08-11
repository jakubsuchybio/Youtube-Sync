using System;
using System.Reflection;
using Easy.Common;
using Serilog;
using Serilog.Exceptions;
using Topshelf;

[assembly: AssemblyMetadata("SquirrelAwareVersion", "1")]

namespace Youtube_Sync
{
    public class Program
    {
        private const string LOGGER_TEMPLATE = "{Timestamp:HH:mm:ss.fffffff zzz} [{Level:u3}] [{ThreadId}] {Message:j}{NewLine}{Exception}";

        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .Enrich.WithDemystifiedStackTraces()
                .WriteTo.Console(outputTemplate: LOGGER_TEMPLATE)
                .WriteTo.File("C:/ProgramData/youtube-sync.txt", outputTemplate: LOGGER_TEMPLATE, shared: true)
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var diagnostics = DiagnosticReport.Generate(DiagnosticReportType.Process | DiagnosticReportType.System);
            Log.Information(diagnostics.ToString());

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

                hostConfig.AddCommandLineSwitch("squirrel", x => { Environment.Exit(0); });
                hostConfig.AddCommandLineDefinition("firstrun", x => { Environment.Exit(0); });
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
