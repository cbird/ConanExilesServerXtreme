using System;
using System.IO;
using CESX.Helpers;
using CESX.Settings;

namespace CESX.Server
{
    public class ServerRunner : IDisposable
    {
        private readonly CesxSettings _settings;
        private ProcessWrapper _process;

        public ServerRunner(CesxSettings settings)
        {
            _settings = settings;
        }

        public bool IsFirstTime()
            => !Directory.Exists(_settings.ServerSavedDir);
        
        public void Run()
        {
            if (!File.Exists(_settings.ServerExePath))
                throw new NotSupportedException(
                    $"There is no executable for the server on given path {_settings.ServerExePath}");

            _process = ProcessWrapper
                .Create(_settings.ServerExePath)
                .WithArgs("-server", "-log")
                .Start();
            
            _process.OutputDataReceived += (e, args) => Console.WriteLine(args.Data);
            _process.ErrorDataReceived += (e, args) => Console.Error.WriteLine(args.Data);

            _process.Wait();
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}