using System.Diagnostics;
using Serilog;

namespace Youtube_Sync
{
    public static class Utils
    {

        public static int ProcessRun(string filePath, string arguments, int timeout)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(startInfo))
            {
                if (process == null)
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
