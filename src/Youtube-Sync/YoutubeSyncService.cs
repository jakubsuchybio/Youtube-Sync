using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Topshelf.Squirrel.Updater.Interfaces;

namespace Youtube_Sync
{
    public class YoutubeSyncService : ISelfUpdatableService
    {
        private readonly CancellationTokenSource _cts;
        private readonly SemaphoreSlim _semaphore;

        private readonly string _youtubeDlPath = @"c:\Users\jakub\Downloads\youtube-dl\youtube-dl.exe";
        private readonly string _outputDir = @"c:\Users\jakub\Downloads\youtube-dl\WL\";

        public YoutubeSyncService()
        {
            _cts = new CancellationTokenSource();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async void Start()
        {
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
                await _semaphore.WaitAsync();
                try
                {
                    Log.Information("Updating youtube-dl starts");

                    ProcessRun("--update", 1000 * 60);

                    Log.Information("Updating youtube-dl ended");
                }
                finally
                {
                    //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    _semaphore.Release();
                }

                await Task.Delay(TimeSpan.FromHours(12));
            }
        }

        private async Task DumbAutoDownloadingTask()
        {
            while (!_cts.IsCancellationRequested)
            {
                await _semaphore.WaitAsync();
                try
                {
                    Log.Information("Downloading of Watch Later starts");
                    
                    ProcessRun($@"--netrc --ignore-errors -o {_outputDir}%(id)s-%(resolution)s-%(uploader)s-%(title)s.%(ext)s https://www.youtube.com/playlist?list=WL", 1000 * 60 * 60 * 24);
                    
                    Log.Information("Downloading of Watch Later ended");
                }
                finally
                {
                    //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    _semaphore.Release();
                }

                await Task.Delay(1000);
            }
        }

        public int ProcessRun(string arguments, int timeout)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _youtubeDlPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(startInfo))
            {
                if(process == null)
                    return -1;

                process.OutputDataReceived += (s, e) => Log.Information(e.Data);
                process.ErrorDataReceived += (s, e) => Log.Error(e.Data);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit(timeout);

                return process.ExitCode;
            }
        }
    }
}
