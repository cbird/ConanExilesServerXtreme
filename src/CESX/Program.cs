using System;
using System.Diagnostics;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using CESX.Helpers;
using CESX.Server;
using CESX.Settings;

namespace CESX
{
    /// <summary>
    /// ConanExilesServer
    /// </summary>
    class Program
    {
        private static bool _disposing = false;
        private static ServerRunner _server;
        private static ServerUpdater _updater;
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        
        ~Program()
        {
            Dispose();
        }

        [Flags]
        enum ExitCode
        {
            Success = 0,
            UnknownError = 100
        }
        
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Statuscode</returns>
        public static Task<int> Main(string[] args) 
        {
            var exitCode = ExitCode.Success;

            // run application
            try
            {
                AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
                {
                    Console.WriteLine("CESX: Exiting");
                    Dispose();
                };
                AssemblyLoadContext.Default.Unloading += ctx =>
                {
                    Console.WriteLine("CESX: Unloading");
                    Dispose();
                };
                Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLine("CESX: Cancel key pressed");
                    Dispose();
                };

                return Execute(args, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                exitCode = ExitCode.Success;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"CESX: {ex.Message}");
                Trace.Write(ex);
                exitCode = ExitCode.UnknownError;
            }
            
            return Task.FromResult((int) exitCode);
        }

        private static async Task<int> Execute(string[] args, CancellationToken token)
        {
            var settings = SettingsParser.ParseArgs(CesxSettings.Default, args);
            
            Console.WriteLine("CESX: starting with settings");
            Console.WriteLine(settings);
            
            if (!settings.SkipBackup)
            {
                Console.WriteLine("CESX: Creating backup...");
                var backuper = new ServerBackuper(settings);
                backuper.DoBackup();
                Console.WriteLine("CESX: Done!");
            }
            
            if (!settings.SkipUpdate)
            {
                Console.WriteLine("CESX: Try updating server files");
                _updater = new ServerUpdater(settings); 
                
                Console.WriteLine("CESX: Downloading SteamCmd...");
                await _updater.EnsureSteamCmdExistsAsync(token);
                Console.WriteLine("CESX: Done!");
            
                Console.WriteLine("CESX: Running steamcmd...");
                await _updater.RunSteamCmdUpdaterAsync(token);
                Console.WriteLine("CESX: Done!");
                
                //_updater.Dispose();
            }
            
            Console.WriteLine("CESX: Starting the server instance");
            _server = new ServerRunner(settings);
            _server.Run();
            
            return (int)ExitCode.Success;
        }

        private static void Dispose()
        {
            if (_disposing)
                return;

            _server?.Dispose();
            _updater?.Dispose();
            _cts.Cancel();
            _disposing = true;
        }
    }
}