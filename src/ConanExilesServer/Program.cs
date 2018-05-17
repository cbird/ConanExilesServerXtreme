using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConanExilesServer.Helpers;

namespace ConanExilesServer
{
    /// <summary>
    /// ConanExilesServer
    /// </summary>
    class Program
    {
        [Flags]
        private enum ExitCode
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
            var counter = 1;
            Console.Write($"Idle");
            while (!token.IsCancellationRequested)
            {
                Console.Write($".");
                await Task.Delay(1000, token);
                counter++;
            }
            return ExitCode.Success;
        }
    }
}