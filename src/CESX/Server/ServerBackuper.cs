using System;
using System.IO;
using System.IO.Compression;
using CESX.Settings;

namespace CESX.Server
{
    public class ServerBackuper
    {
        private readonly CesxSettings _settings;

        public ServerBackuper(CesxSettings settings)
        {
            _settings = settings;
        }
        
        public void DoBackup()
        {
            var inputDirectory = _settings.ServerSavedDir;
            var outputFilename = Path.Combine(_settings.ServerBackupDir, GetFilename());
            
            if (!Directory.Exists(inputDirectory))
                Directory.CreateDirectory(inputDirectory);

            if (File.Exists(outputFilename))
                return; // If a backup is happening on the exact time again. That is not needed
            
            if (!Directory.Exists(_settings.ServerBackupDir))
                Directory.CreateDirectory(_settings.ServerBackupDir);

            ZipFile.CreateFromDirectory(inputDirectory, outputFilename, CompressionLevel.Optimal, false);
        }

        private string GetFilename()
        {
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, _settings.TimeZone);
            return $"saved_{now:yyyyMMddHHmm}.zip";
        }
    }
}