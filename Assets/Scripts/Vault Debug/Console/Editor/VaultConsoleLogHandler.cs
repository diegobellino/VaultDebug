using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Assertions;
using VaultDebug.Logging.Runtime;

namespace VaultDebug.Console.Editor
{
    public class VaultConsoleLogHandler : IDisposable, IVaultLogHandler
    {

        #region VARIABLES

        const string ACTIVE_FILTERS_KEY = "Vault.Logging.VaultConsoleEditor";
        const string COMPILER_MESSAGE_PATTERN = @"^(.*)\((\d{2}),\d{2}\):\s(.*)";

        public Action OnLogsChanged;

        LogLevel _activeFilters;
        Dictionary<int, VaultLog> _allLogsById = new();
        List<IVaultLogListener> _listeners = new();

        int _logCount;

        #endregion

        ~VaultConsoleLogHandler()
        {
            Dispose(false);
        }

        public void Init()
        {
            Application.logMessageReceived += HandleUnityLog;
            AssemblyReloadEvents.beforeAssemblyReload += ClearLogs;
            CompilationPipeline.assemblyCompilationFinished += HandleCompilationLogs;
            VaultLogDispatcher.Instance.RegisterHandler(this);

            ReadPreferences();
        }

        public void RegisterListener(IVaultLogListener listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(IVaultLogListener listener)
        {
            if (_listeners.Contains(listener))
            {
                _listeners.Remove(listener);
            }
        }

        #region LOGGING

        public void HandleVaultLog(VaultLog log)
        {
            _allLogsById.Add(_logCount, log);
            _logCount++;
            RefreshListeners();
        }

        void HandleUnityLog(string logMessage, string stackTrace, LogType type)
        {
            var assignedLevel = type switch
            {
                LogType.Error => LogLevel.Error,
                LogType.Warning => LogLevel.Warn,
                LogType.Log => LogLevel.Info,
                LogType.Exception or LogType.Assert => LogLevel.Exception,
                _ => LogLevel.Info
            };

            // Discard log, as it's a compiler error and will be handled differently
            if (logMessage.IsMatch(COMPILER_MESSAGE_PATTERN))
            {
                return;
            }

            var log = new VaultLog(assignedLevel, "UNITY LOG", logMessage, stackTrace);
            _allLogsById.Add(_logCount, log);
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

                var log = new VaultLog(LogLevel.Exception, "COMPILATION", message, path + $"(line {line})");
                _allLogsById.Add(_logCount, log);
                _logCount++;
            }

            RefreshListeners();
        }

        public Dictionary<int, VaultLog> GetLogsFiltered()
        {
            var filteredLogs = new Dictionary<int, VaultLog>();

            foreach(var kvp in _allLogsById)
            {
                var id = kvp.Key;
                var log = kvp.Value;

                if (!IsFilterActive(log.Level))
                {
                    continue;
                }
                
                filteredLogs.Add(id, log);
            }

            return filteredLogs;
        }

        public VaultLog GetLogWithId(int id)
        {
            Assert.IsTrue(_allLogsById.TryGetValue(id, out var log), $"Log with id {id} not found");
            return log;
        }


        void ClearLogs()
        {
            _allLogsById.Clear();
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
                WritePreferences();
    
                VaultLogDispatcher.Instance.UnregisterHandler(this);

                _allLogsById.Clear();
                _allLogsById = null;
            }
        }

        #endregion
    }
}