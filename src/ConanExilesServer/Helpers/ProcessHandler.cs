using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesServer.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessHandler
    {
        private readonly TextWriter _standardOutput;
        private readonly TextWriter _errorOuput;

        public ProcessHandler(TextWriter standardOutput, TextWriter errorOuput)
        {
            _standardOutput = standardOutput ?? throw new ArgumentNullException(nameof(standardOutput));
            _errorOuput = errorOuput ?? throw new ArgumentNullException(nameof(errorOuput));
        }

        public static ProcessHandler Default => new ProcessHandler(Console.Out, Console.Error);
        
        public Task<int> RunProcessAsync(string fileName, string args, CancellationToken cancellationToken)
        {
            using (var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            })
            {
                return RunProcessAsync(process, cancellationToken);
            }
        }

        private Task<int> RunProcessAsync(Process process, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<int>();
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                process.Exited += (s, ea) => tcs.SetResult(process.ExitCode);
                process.OutputDataReceived += (s, ea) => _standardOutput.WriteLine(ea.Data);
                process.ErrorDataReceived += (s, ea) => _errorOuput.WriteLine(ea.Data);

                if (!process.Start())
                    throw new InvalidOperationException("Could not start process: " + process);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                return tcs.Task;
            }
        }
    }
}