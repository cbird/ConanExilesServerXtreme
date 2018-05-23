using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConanExilesServer.Helpers;
using ConanExilesServer.Server;
using ConanExilesServer.Settings;
using Microsoft.Extensions.Options;

namespace ConanExilesServer
{
    /// <summary>
    /// ConanExilesServer
    /// </summary>
    class Program
    {
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
        static int Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            // Listen on cancellation
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine();
                Console.WriteLine("Exiting...");
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                Console.WriteLine("Starting ConanExilesServer...");
                Console.WriteLine("Press Ctrl+C to exit");
                
                // Run async
                return (int) AsyncHelper.RunSync(() => MainAsync(args, cts.Token));
            }
            catch (OperationCanceledException ex)
            {
                //Console.WriteLine($"OperationCanceled: {ex.Message}");
                return (int) ExitCode.Success;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Trace.Write(ex);
                return (int) ExitCode.UnknownError;
            }
        }

        static async Task<ExitCode> MainAsync(string[] args, CancellationToken token)
        {
            var options = Options.Create(new ServerUpdaterSettings
            {
                SteamCmdInstallDir = @"c:\temp\steamcmd\",
                SteamCmdDownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip",
                ServerInstallDir = @"c:\temp\conan_server\"
            });
            var updater = new ServerUpdater(options);
            
            await updater.EnsureSteamCmdExistsAsync(token);
            await updater.RunSteamCmdUpdaterAsync(token);

            Console.WriteLine("Done! Exiting ConanServer...");
            
            return ExitCode.Success;
        }
    }
}