using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CESX.Helpers;
using CESX.Settings;
using Microsoft.Extensions.Options;

namespace CESX.Server
{
    public class ServerUpdater : IDisposable
    {
        private const int ConanExilesServerSteamAppId = 443030;
        private readonly CesxSettings _settings;
        private ProcessWrapper _process;

        public ServerUpdater(CesxSettings settings)
        {
            _settings = settings;
        }

        public async Task EnsureSteamCmdExistsAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(_settings.SteamCmdPath))
                Directory.Delete(_settings.SteamCmdInstallDir, true); // always delete before otherwise it could end up in a weird state "Waiting for user info...OK"

            if (!_settings.SteamCmdDownloadUrl.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                throw new NotSupportedException(
                    $"The {nameof(_settings.SteamCmdDownloadUrl)} is expected to point to a zip-file. Currently it is: {_settings.SteamCmdDownloadUrl}");

            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(_settings.SteamCmdDownloadUrl, cancellationToken))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var zip = new ZipArchive(stream))
            {
                Directory.CreateDirectory(_settings.SteamCmdInstallDir);
                zip.ExtractToDirectory(_settings.SteamCmdInstallDir);
            }
        }

        public Task RunSteamCmdUpdaterAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_settings.SteamCmdPath))
                throw new NotSupportedException(
                    $"There is no executable for steamcmd on given path {_settings.SteamCmdPath}");

            return Task.Factory.StartNew(() =>
            {
                if (!Directory.Exists(_settings.ServerInstallDir))
                    Directory.CreateDirectory(_settings.ServerInstallDir);

                var steamCmdArgs =
                    $"+login anonymous +force_install_dir \"{_settings.ServerInstallDir}\" +app_update {ConanExilesServerSteamAppId} +quit";

                _process = ProcessWrapper.Create(_settings.SteamCmdPath)
                    .WithArgs(steamCmdArgs)
                    .Start();

                _process.OutputDataReceived += (e, args) => Console.WriteLine(args.Data);
                _process.ErrorDataReceived += (e, args) => Console.Error.WriteLine(args.Data);

                _process.Wait();
            }, cancellationToken);
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}