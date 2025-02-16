using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using VaultDebug.Console.Editor.Utils;
using VaultDebug.Logging.Runtime;
using Newtonsoft.Json;
using System.IO;

namespace VaultDebug.Console.Editor
{
    public class VaultConsoleLogHandler : IDisposable, IVaultLogHandler
    {

        #region VARIABLES

        const string ACTIVE_FILTERS_KEY = "Vault.Logging.VaultConsoleEditor";
        const string COMPILER_MESSAGE_PATTERN = @"^(.*)\((\d{2}),\d{2}\):\s(.*)";
        const string CONTEXT_FILTER_PATTERN = @"@context:\s*""(.*?)""|@context:\s*(\S+)";
        const int MAX_LOGS = 1000;
        const string LOG_FILE_PATH = "vault_logs.json";
        string FullLogPath => Path.Combine(Application.persistentDataPath, LOG_FILE_PATH);

        public Action OnLogsChanged;

        LogLevel _activeFilters = LogLevel.Debug & LogLevel.Error & LogLevel.Info & LogLevel.Warn;
        Dictionary<LogLevel, List<VaultLog>> _logsByLevel = new()
        {
            { LogLevel.Info, new List<VaultLog>() },
            { LogLevel.Debug, new List<VaultLog>() },
            { LogLevel.Warn, new List<VaultLog>() },
            { LogLevel.Error, new List<VaultLog>() },
            { LogLevel.Exception, new List<VaultLog>() }
        };
        List<IVaultLogListener> _listeners = new();

        int _logCount;

        #endregion

        ~VaultConsoleLogHandler()
        {
            Dispose(false);
        }

        public void Init()
        {
            Application.logMessageReceivedThreaded += HandleUnityLog;
            AssemblyReloadEvents.beforeAssemblyReload += ClearLogs;
            CompilationPipeline.assemblyCompilationFinished += HandleCompilationLogs;
            VaultLogDispatcher.RegisterHandler(this);

            ReadPreferences();

            LoadLogs();
        }

        public void RegisterLogListener(IVaultLogListener listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterLogListener(IVaultLogListener listener)
        {
            if (_listeners.Contains(listener))
            {
                _listeners.Remove(listener);
            }
        }

        #region LOAD/SAVE

        void SaveLogs()
        {
            var logs = new List<VaultLog>();

            foreach (var logList in _logsByLevel.Values)
            {
                logs.AddRange(logList);
            }

            logs.Sort();

            string json = JsonConvert.SerializeObject(logs, Formatting.Indented);
            File.WriteAllText(FullLogPath, json);
        }

        void LoadLogs()
        {
            if (!File.Exists(FullLogPath)) return;

            string json = File.ReadAllText(FullLogPath);
            var logs = JsonConvert.DeserializeObject<List<VaultLog>>(json);

            if (logs != null)
            {
                foreach (var log in logs)
                {
                    _logsByLevel[log.Level].Add(log);
                }
            }

            RefreshListeners();
        }

        #endregion

        #region LOGGING

        public void HandleLog(VaultLog log)
        {
            var logs = _logsByLevel[log.Level];
            logs.Add(log);
            _logCount++;

            if (_logCount > MAX_LOGS)
            {
                logs.RemoveAt(0); // Remove oldest log
            }

            RefreshListeners();
        }

        void HandleUnityLog(string logMessage, string stackTrace, LogType type)
        {
            if (Application.isPlaying)
            {
                VaultDebugMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    ProcessUnityLog(logMessage, stackTrace, type);
                });
            }
            else
            {
                ProcessUnityLog(logMessage, stackTrace, type);
            }
        }

        void ProcessUnityLog(string logMessage, string stackTrace, LogType type)
        {
            var assignedLevel = type switch
            {
                LogType.Error => LogLevel.Error,
                LogType.Warning => LogLevel.Warn,
                LogType.Log => LogLevel.Info,
                LogType.Exception or LogType.Assert => LogLevel.Exception,
                _ => LogLevel.Info
            };

            // Filtrar errores de compilación para evitar duplicados
            if (logMessage.IsMatch(COMPILER_MESSAGE_PATTERN))
            {
                return;
            }


            var log = VaultLogPool.GetLog(assignedLevel, "UNITY LOG", logMessage, stackTrace);
            _logsByLevel[log.Level].Add(log);
            _logCount++;

            RefreshListeners();
        }

        void HandleCompilationLogs(string s, CompilerMessage[] compilerMessages)
        {
            foreach (var compilerMessage in compilerMessages)
            {
                // For now, filter out anything non-error from console
                if (compilerMessage.type != CompilerMessageType.Error)
                {
                    continue;
                }

                // Patter has 3 groups: Script path, code line and message
                var matchedGroups = compilerMessage.message.MatchOnce(COMPILER_MESSAGE_PATTERN);
                // Regex returns raw string as first match for some reason
                var rawMessage = matchedGroups[0].ToString();
                var path = matchedGroups[1].ToString();
                var line = matchedGroups[2].ToString();
                var message = matchedGroups[3].ToString();


                _logCount++;

                var log = new VaultLog(LogLevel.Exception, "COMPILATION", message, path + $"(line {line})");
                _logsByLevel[log.Level].Add(log);
            }

            RefreshListeners();
        }

        public List<VaultLog> GetLogsFiltered(string textFilter)
        {
            var filteredLogs = new List<VaultLog>();

            if (!string.IsNullOrEmpty(textFilter))
            {
                var contextFilterMatches = Regex.Matches(textFilter, CONTEXT_FILTER_PATTERN);
                var contextFilters = new List<string>();

                foreach (Match match in contextFilterMatches)
                {
                    var contextToFilter = match.Groups[1].Value;
                    contextFilters.Add(contextToFilter);
                }

                foreach (var level in _logsByLevel.Keys)
                {
                    if (IsFilterActive(level))
                    {
                        foreach (var log in _logsByLevel[level])
                        {
                            if (contextFilters.Contains(log.Context))
                            {
                                filteredLogs.Add(log);
                            }
                        }
                    }
                }

                return filteredLogs;
            }

            foreach(var level in _logsByLevel.Keys)
            {
                if (IsFilterActive(level))
                {
                    filteredLogs.AddRange(_logsByLevel[level]);
                }
            }

            return filteredLogs;
        }

        public VaultLog GetLogWithId(int id)
        {
            foreach (var logsInLevel in _logsByLevel.Values)
            {
                foreach (var log in logsInLevel)
                {
                    if (log.Id == id)
                    {
                        return log;
                    }
                }
            }

            throw new KeyNotFoundException($"VaultConsole could not find a log with id {id}");
        }


        public void ClearLogs()
        {
            foreach (var level in _logsByLevel.Keys)
            {
                _logsByLevel[level].Clear();
            }

            _logCount = 0;
            SaveLogs();
            RefreshListeners();
        }

        void RefreshListeners()
        {
            foreach(var listener in _listeners)
            {
                listener.RefreshLogs();
            }
        }

        #endregion

        #region FILTERING

        public void TriggerFilter(LogLevel logLevel)
        {
            if (_activeFilters.HasFlag(logLevel))
            {
                _activeFilters &= ~logLevel;
            }
            else
            {
                _activeFilters |= logLevel;
            }
        }

        public bool IsFilterActive(LogLevel logLevel)
        {
            return logLevel == LogLevel.Exception || _activeFilters.HasFlag(logLevel);
        }

        #endregion

        #region PREFERENCES

        void ReadPreferences()
        {
            _activeFilters = (LogLevel)EditorPrefs.GetInt(ACTIVE_FILTERS_KEY, 0);
        }

        void WritePreferences()
        {
            EditorPrefs.SetInt(ACTIVE_FILTERS_KEY, (int)_activeFilters);
        }

        #endregion

        #region DISPOSABLE

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposeManagedResources)
        {
            Application.logMessageReceived -= HandleUnityLog;
            AssemblyReloadEvents.beforeAssemblyReload -= ClearLogs;
            CompilationPipeline.assemblyCompilationFinished -= HandleCompilationLogs;

            if (disposeManagedResources)
            {
                SaveLogs();
                WritePreferences();
    
                VaultLogDispatcher.UnregisterHandler(this);

                _logsByLevel.Clear();
            }
        }

        #endregion
    }
}