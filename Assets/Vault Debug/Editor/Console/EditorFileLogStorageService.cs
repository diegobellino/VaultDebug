using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Editor.Console
{
    public class EditorFileLogStorageService : ILogStorageService
    {
        const string LOG_FILE_NAME = "vault_logs.json";
        string FullLogPath => Path.Combine(Application.persistentDataPath, LOG_FILE_NAME);

        public async Task SaveLogsAsync(IEnumerable<VaultLog> logs)
        {
            var json = JsonConvert.SerializeObject(logs, Formatting.Indented);
            await File.WriteAllTextAsync(FullLogPath, json);
        }

        public async Task<IEnumerable<VaultLog>> LoadLogsAsync()
        {
            if (!File.Exists(FullLogPath))
                return new List<VaultLog>();

            string json = await File.ReadAllTextAsync(FullLogPath);
            var logs = JsonConvert.DeserializeObject<List<VaultLog>>(json);
            return logs ?? new List<VaultLog>();
        }

        public async Task ExportLogsAsync(IEnumerable<VaultLog> logs, string exportPath)
        {
            var orderedLogs = logs.OrderBy(x => x.Id);
            var logStrings = new List<string>();
            foreach (var log in orderedLogs)
            {
                logStrings.Add($"[{new DateTime(log.TimeStampTicks).ToString("HH:mm:ss.ffff")}] {log.Context}: {log.Message}");
            }

            string exportFile = Path.Combine(exportPath, "vault_logs_export.txt");
            await File.WriteAllLinesAsync(exportFile, logStrings);
            Debug.Log($"Logs exported to: {exportFile}");
        }
    }
}
