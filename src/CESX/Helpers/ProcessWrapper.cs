using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CESX.Helpers
{
    public class ProcessWrapper : IDisposable
    {
        private readonly object _eventLock = new object();
        private readonly ProcessStartInfo _startInfo;
        private Process _process;

        private ProcessWrapper(string fileName)
        {
            _startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(fileName)
            };
        }

        public static ProcessWrapper Create(string path)
            => new ProcessWrapper(path);
        
        public ProcessWrapper WithArgs(params string[] args)
        {
            if (args.Any())
            {
                _startInfo.Arguments = string.Join(" ", args);
            }

            return this;
        }

        ~ProcessWrapper()
        {
            Dispose(false);
        }

        public event EventHandler<DataReceivedEventArgs> OutputDataReceived = delegate { };

        public event EventHandler<DataReceivedEventArgs> ErrorDataReceived = delegate { };
        
        public event EventHandler<EventArgs> Exited = delegate { };

        public int ExitCode => _process.ExitCode;

        //public ProcessStartInfo StartInfo => _process.StartInfo;

        public ProcessWrapper Start()
        {
            try
            {
                _process = Process.Start(_startInfo) ?? throw new InvalidOperationException($"Could not start process with fileName '{_startInfo.FileName}'");
                _process.OutputDataReceived += OnOutputDataReceived;
                _process.ErrorDataReceived += OnErrorDataReceived;
                _process.Exited += OnExited;
                _process.BeginErrorReadLine();
                _process.BeginOutputReadLine();
            }
            catch (Win32Exception ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }

            return this;
        }
        
        private ProcessWrapper Kill()
        {
            try
            {
                if (!_process.HasExited)
                {
                    Console.WriteLine($"CESX: Stopping process '{_process.Id}' with name '{_process.ProcessName}'.");
                    _process.Kill();
                }
                
//                if (_process.HasExited)
//                    return this;
//                
//                Console.WriteLine($"Stopping process '{_process.Id}' with name '{_process.ProcessName}'.");
//                _process.Kill();

                try
                {
                    foreach (var proc in Process.GetProcesses()
                        .Where(p => p.Id != _process.Id && p.ProcessName.Contains(_process.ProcessName)))
                    {
                        if (proc.HasExited)
                            continue;

                        Console.WriteLine($"CESX: Stopping process '{proc.Id}' with name '{proc.ProcessName}'.");

                        // send ctrl+c
                        ProcessStopper.StopProgram(proc.Id, 5000);
                        //proc.Kill();
                    }

                }
                catch (InvalidOperationException)
                {
                    // This happens when the spawned ConanSandboxServer window is closed. It should be fine, so return
                    return this;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            return this;
        }

        public ProcessWrapper Wait()
        {
            _process.WaitForExit();
            return this;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) 
                return;
            
            Kill();
            _process.Dispose();
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_eventLock)
            {
                ErrorDataReceived(sender, e);
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_eventLock)
            {
                OutputDataReceived(sender, e);
            }
        }

        private void OnExited(object sender, EventArgs e)
        {
            lock (_eventLock)
            {
                Exited(sender, e);
            }
        }
    }
}