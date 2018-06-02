using CESX.Helpers;
using CESX.Settings;
using Xunit;

namespace ConanExilesServer.Tests.Helpers
{
    public class SettingsParserShould
    {
        [Fact]
        public void ReturnDefaultValues()
        {
            var settings = SettingsParser.ParseArgs(CesxSettings.Default, null);
            
            Assert.Equal(@"C:\ConanServer\Backups\", settings.ServerBackupDir);
            Assert.Equal(@"C:\ConanServer\Server\", settings.ServerInstallDir);
            Assert.Equal(@"https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip", settings.SteamCmdDownloadUrl);
            Assert.Equal(@"C:\ConanServer\Updater\", settings.SteamCmdInstallDir);
            Assert.Equal("W. Europe Standard Time", settings.TimeZone);
        }

        [Fact]
        public void ReturnCorrectValuesAccordingToArgs()
        {
            var serverBackupDir = "1";
            var serverInstallDir = "2";
            var steamcmdDownloadUrl = "3";
            var steamcmdInstallDir = "4";
            var timeZone = "5";
            
            var settings = SettingsParser.ParseArgs(CesxSettings.Default, new[]
            {
                "--server_backup_dir",
                serverBackupDir,
                "--server_install_dir",
                serverInstallDir,
                "--steamcmd_download_url",
                steamcmdDownloadUrl,
                "--steamcmd_install_dir",
                steamcmdInstallDir,
                "--time_zone",
                timeZone,
                "--skip_backup",
                "--skip_update"
            });

            Assert.Equal(serverBackupDir, settings.ServerBackupDir);
            Assert.Equal(serverInstallDir, settings.ServerInstallDir);
            Assert.Equal(steamcmdDownloadUrl, settings.SteamCmdDownloadUrl);
            Assert.Equal(steamcmdInstallDir, settings.SteamCmdInstallDir);
            Assert.Equal(timeZone, settings.TimeZone);
            Assert.True(settings.SkipBackup);
            Assert.True(settings.SkipUpdate);
        }
    }
}