using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CESX.Settings
{
    public class CesxSettings
    {
        public static CesxSettings Default
            => new CesxSettings
            {
                ServerBackupDir = @"C:\ConanServer\Backups\",
                ServerInstallDir = @"C:\ConanServer\Server\",
                SkipUpdate = false,
                SteamCmdDownloadUrl = @"https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip",
                SteamCmdInstallDir = @"C:\ConanServer\Updater\",
                TimeZone = "W. Europe Standard Time"
            };


        [Required] public string ServerBackupDir { get; set; }

        [Required] public string ServerInstallDir { get; set; }

        [Required] public bool SkipBackup { get; set; }

        [Required] public bool SkipUpdate { get; set; }

        [Required] public string SteamCmdDownloadUrl { get; set; }

        [Required] public string SteamCmdInstallDir { get; set; }

        [Required] public string TimeZone { get; set; }


        public string SteamCmdPath => Path.Combine(SteamCmdInstallDir, "steamcmd.exe");

        public string ServerExePath => Path.Combine(ServerInstallDir, "ConanSandboxServer.exe");

        public string ServerSavedDir => Path.Combine(ServerInstallDir, "ConanSandbox", "Saved");

        public override string ToString()
            =>
                $"ServerBackupDir: '{ServerBackupDir}', ServerInstallDir: '{ServerInstallDir}', SkipBackup: '{SkipBackup}', SkipUpdate: '{SkipUpdate}', SteamCmdDownloadUrl: '{SteamCmdDownloadUrl}', SteamCmdInstallDir: '{SteamCmdInstallDir}', TimeZone: '{TimeZone}'";
    }
}