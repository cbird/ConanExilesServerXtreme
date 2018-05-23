using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConanExilesServer.Helpers;
using ConanExilesServer.Server;
using ConanExilesServer.Settings;
using ConanExilesServer.Tests.TestHelpers;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace ConanExilesServer.Tests.Server
{
    public class ServerUpdaterShould
    {
        private readonly ITestOutputHelper _output;
        private readonly IOptions<ServerUpdaterSettings> _options;
        private readonly ProcessHandler _processHandler;
        private readonly CancellationTokenSource _cts;
            
        public ServerUpdaterShould(ITestOutputHelper output)
        {
            _output = output;
            _options = Options.Create(new ServerUpdaterSettings
            {
                SteamCmdInstallDir = @"c:\temp\steamcmd\",
                SteamCmdDownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip",
                ServerInstallDir = @"c:\temp\conan_server\"
            });
            var writer = new TestOutputTextWriter(_output);
            _processHandler = new ProcessHandler(writer, writer);
            _cts = new CancellationTokenSource();
        }
        
        ~ServerUpdaterShould()
        {
            _cts.Cancel();
        }
        
        [RunnableInDebugOnly]
        public async Task DownloadAndExtractSteamCmdToTempPath()
        {
            // Arrange
            var updater = new ServerUpdater(_options);

            // Act
            if(Directory.Exists(_options.Value.SteamCmdInstallDir))
                Directory.Delete(_options.Value.SteamCmdInstallDir, true);
            
            await updater.EnsureSteamCmdExistsAsync(_cts.Token);

            // Assert
            Assert.True(File.Exists(_options.Value.SteamCmdPath));
        }

        [RunnableInDebugOnly]
        public async Task RunSteamCmdUpdaterAndInstallToTempPath()
        {
            // Arrange
            var updater = new ServerUpdater(_options);

            // Act
            if(Directory.Exists(_options.Value.SteamCmdInstallDir))
                Directory.Delete(_options.Value.SteamCmdInstallDir, true);
            
            await updater.EnsureSteamCmdExistsAsync(_cts.Token);
            await updater.RunSteamCmdUpdaterAsync(_cts.Token);
            
            // Assert
            Assert.True(File.Exists(Path.Combine(_options.Value.ServerInstallDir, "StartServer.bat")));
        }

        private class TestOutputTextWriter : TextWriter
        {
            private readonly ITestOutputHelper _helper;

            public TestOutputTextWriter(ITestOutputHelper helper)
            {
                _helper = helper;
                Encoding = Encoding.UTF8;
            }
            public override Encoding Encoding { get; }

            public override void WriteLine(string value)
            {
                _helper.WriteLine(value);
                Console.WriteLine(value);
                Debug.WriteLine(value);
            }
        }
    }
}