using System;
using System.Timers;
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

        private class YoutubeSyncService
        {
            private readonly Timer _timer;

            public YoutubeSyncService()
            {

                _timer = new Timer { AutoReset = true, Interval = 30000 };
                _timer.Elapsed += Timer_Elapsed;
            }

            private void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                
            }

            public void Start()
            {
                _timer.Start();
            }

            public void Stop()
            {
                _timer.Stop();
            }
        }
    }
}
