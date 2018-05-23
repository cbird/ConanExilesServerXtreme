using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ConanExilesServer.Settings
{
    public class ServerUpdaterSettings
    {
        [Required]
        public string SteamCmdDownloadUrl { get; set; }
        
        [Required]
        public string SteamCmdInstallDir { get; set; }
        
        [Required]
        public string ServerInstallDir { get; set; }
        
        public string SteamCmdPath => Path.Combine(SteamCmdInstallDir, "steamcmd.exe");
    }
}