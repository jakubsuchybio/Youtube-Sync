using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Youtube_Sync
{
    public class YoutubeSyncService
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly AsyncLock _youtubeDlSemaphore = new AsyncLock();

        private const string YoutubeDlPath = @"c:\Users\jakub\Downloads\youtube-dl\youtube-dl.exe";
        private const string OutputDir = @"c:\Users\jakub\Downloads\youtube-dl\WL\";

        public async void Start()
        {
            var home = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(home))
                Environment.SetEnvironmentVariable("HOME", @"c:\Users\jakub\");

            _ = Task.Factory.StartNew(YoutubeDlAutoUpdatingTask, TaskCreationOptions.LongRunning);
            await Task.Delay(1000);
            _ = Task.Factory.StartNew(DumbAutoDownloadingTask, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private async Task YoutubeDlAutoUpdatingTask()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    using (await _youtubeDlSemaphore.LockAsync())
                    {
                        Log.Information("Updating youtube-dl starts");

                        Utils.ProcessRun(YoutubeDlPath, "--update", 1000 * 60);

                        Log.Information("Updating youtube-dl ended");
                        throw new Exception("test");
                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "YoutubeDlAutoUpdatingTask crashed");
                }

                await Task.Delay(TimeSpan.FromHours(12));
            }
        }

        private async Task DumbAutoDownloadingTask()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    using (await _youtubeDlSemaphore.LockAsync())
                    {
                        Log.Information("Downloading of Watch Later starts");

                        Utils.ProcessRun(YoutubeDlPath, $@"--netrc --ignore-errors -o {OutputDir}%(id)s-%(resolution)s-%(uploader)s-%(title)s.%(ext)s https://www.youtube.com/playlist?list=WL", 1000 * 60 * 60 * 24);

                        Log.Information("Downloading of Watch Later ended");
                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "DumbAutoDownloadingTask crashed");
                }

                await Task.Delay(15000);
            }
        }
    }
}
