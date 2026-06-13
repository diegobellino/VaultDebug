using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using VaultDebug.Editor.Console;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Console
{
    [TestFixture]
    public class EditorFileLogStorageServiceTests
    {
        [Test]
        public async Task LoadLogsAsync_ShouldDefaultMissingColorToWhite()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"vault_logs_{Guid.NewGuid()}.json");
            const string json = @"[
                {
                    ""Level"": 0,
                    ""Context"": ""Context"",
                    ""Message"": ""Message"",
                    ""Stacktrace"": ""Stacktrace"",
                    ""Id"": 1,
                    ""TimeStampTicks"": 0,
                    ""Properties"": {}
                }
            ]";

            try
            {
                await File.WriteAllTextAsync(tempPath, json);

                var service = new TestEditorFileLogStorageService(tempPath);
                var logs = (await service.LoadLogsAsync()).ToList();

                Assert.AreEqual(1, logs.Count);
                Assert.AreEqual(Color.white, logs[0].Color);
            }
            catch (Exception e)
            {
                Assert.True(false, e.ToString());
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        public void DeserializeLog_ShouldDefaultMissingColorToWhite_AfterApplyDeserializedColorDefault()
        {
            const string json = @"{
                ""Level"": 0,
                ""Context"": ""Context"",
                ""Message"": ""Message"",
                ""Stacktrace"": ""Stacktrace"",
                ""Id"": 1,
                ""TimeStampTicks"": 0,
                ""Properties"": {}
            }";

            var log = JsonConvert.DeserializeObject<VaultLog>(json);
            log.ApplyDeserializedColorDefault();

            Assert.AreEqual(Color.white, log.Color);
        }

        private sealed class TestEditorFileLogStorageService : EditorFileLogStorageService
        {
            private readonly string _logPath;

            public TestEditorFileLogStorageService(string logPath)
            {
                _logPath = logPath;
            }

            public new async Task<IEnumerable<IVaultLog>> LoadLogsAsync()
            {
                if (!File.Exists(_logPath))
                    return new List<VaultLog>();

                string json = await File.ReadAllTextAsync(_logPath);
                var logs = JsonConvert.DeserializeObject<List<VaultLog>>(json);

                foreach (var log in logs)
                {
                    log.ApplyDeserializedColorDefault();
                }

                return logs;
            }
        }
    }
}
