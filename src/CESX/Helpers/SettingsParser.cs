using CESX.Settings;

namespace CESX.Helpers
{
    public class SettingsParser
    {
        /// <summary>
        /// Parses command line arguments
        /// </summary>
        /// <param name="settings">Object to append to</param>
        /// <param name="args">CMD args</param>
        /// <returns>Settings object</returns>
        public static CesxSettings ParseArgs(CesxSettings settings, string[] args)
        {
            settings = settings ?? new CesxSettings();

            if (args == null) 
                return settings;
            
            for (var i = 0; i < args.Length; ++i)
            {
                switch (args[i].ToLower())
                {
                    case "--server_backup_dir":
                        settings.ServerBackupDir = i + 1 < args.Length ? args[++i] : settings.ServerBackupDir;
                        break;
                    case "--server_install_dir":
                        settings.ServerInstallDir = i + 1 < args.Length ? args[++i] : settings.ServerInstallDir;
                        break;
                    case "--skip_backup":
                        settings.SkipBackup = true;
                        break;
                    case "--skip_update":
                        settings.SkipUpdate = true;
                        break;
                    case "--steamcmd_download_url":
                        settings.SteamCmdDownloadUrl = i + 1 < args.Length ? args[++i] : settings.SteamCmdDownloadUrl;
                        break;
                    case "--steamcmd_install_dir":
                        settings.SteamCmdInstallDir = i + 1 < args.Length ? args[++i] : settings.SteamCmdInstallDir;
                        break;
                    case "--time_zone":
                        settings.TimeZone = i + 1 < args.Length ? args[++i] : settings.TimeZone;
                        break;
                    default:
                        // argument not known, perhaps log?
                        break;
                }
            }

            return settings;
        }
    }
}