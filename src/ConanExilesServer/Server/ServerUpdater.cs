using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ConanExilesServer.Helpers;
using ConanExilesServer.Settings;
using Microsoft.Extensions.Options;

namespace ConanExilesServer.Server
{
    public class ServerUpdater
    {
        private const int ConanExilesServerSteamAppId = 443030;
        private readonly ServerUpdaterSettings _settings;

        public ServerUpdater(IOptions<ServerUpdaterSettings> options)
        {
            _settings = options.Value;
        }

        public async Task EnsureSteamCmdExistsAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(_settings.SteamCmdPath))
                return;

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

                var proc = new ProcessUtil(_settings.SteamCmdPath, steamCmdArgs);
                proc.Start();

                proc.OutputDataReceived += (e, args) => Console.WriteLine(args.Data);
                proc.ErrorDataReceived += (e, args) => Console.WriteLine(args.Data);

                proc.Wait(cancellationToken);
            }, cancellationToken);
        }
    }
}