using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Editor.Console
{
    public class VaultEditorLogHandler : IDisposable, IVaultLogHandler
    {
        const string COMPILER_MESSAGE_PATTERN = @"^(.*)\((\d{2}),\d{2}\):\s(.*)";
        const string CONTEXT_FILTER_PATTERN = @"@context:\s*""(.*?)""|@context:\s*(\S+)";
        
        public int MaxLogCached { get; set; } = 1000;

        ILogStorageService _logStorageService;
        IVaultLogPool _logPool;
        IVaultLogDispatcher _logDispatcher;
        ILogIdProvider _logIdProvider;

        public Action OnLogsChanged;

        LogLevel _activeFilters = LogLevel.Debug & LogLevel.Error & LogLevel.Info & LogLevel.Warn;
        Dictionary<LogLevel, List<IVaultLog>> _logsByLevel = new()
        {
            { LogLevel.Info, new List<IVaultLog>() },
            { LogLevel.Debug, new List<IVaultLog>() },
            { LogLevel.Warn, new List<IVaultLog>() },
            { LogLevel.Error, new List<IVaultLog>() },
            { LogLevel.Exception, new List<IVaultLog>() }
        };
        Dictionary<string, List<IVaultLog>> _logsByContext = new();
        List<IVaultLogListener> _listeners = new();


        int _logCount;

        public VaultEditorLogHandler()
        {
            Application.logMessageReceivedThreaded += HandleUnityLog;
            AssemblyReloadEvents.beforeAssemblyReload += ClearLogs;
            CompilationPipeline.assemblyCompilationFinished += HandleCompilationLogs;

            _logDispatcher = DIBootstrapper.Container.Resolve<IVaultLogDispatcher>();
            _logPool = DIBootstrapper.Container.Resolve<IVaultLogPool>();
            _logStorageService = DIBootstrapper.Container.Resolve<ILogStorageService>();
            _logIdProvider = DIBootstrapper.Container.Resolve<ILogIdProvider>();

            _logDispatcher.RegisterHandler(this);
        }

        ~VaultEditorLogHandler()
        {
            Dispose(false);
        }

        public async Task InitializeAsync()
        {
            ReadPreferences();
            var logs = await _logStorageService.LoadLogsAsync();

            foreach (var log in logs)
            {
                _logsByLevel[log.Level].Add(log);

                if (!_logsByContext.ContainsKey(log.Context))
                {
                    _logsByContext[log.Context] = new List<IVaultLog>();
                }
                _logsByContext[log.Context].Add(log);
            }
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

        #region LOGGING

        public void HandleLog(IVaultLog log)
        {
            // Logs are reused and transformed by the pool, clone them before cacheing to avoid misconstructions
            var clonedLog = log.Clone();

            if (!Enum.IsDefined(typeof(LogLevel), log.Level))
            {
                Debug.LogWarning($"VaultConsoleLogHandler received an invalid LogLevel: {log.Level}");
                return; // Ignore invalid log levels
            }

            if (string.IsNullOrEmpty(log.Message))
            {
                Debug.LogWarning("VaultConsoleLogHandler received a log with an empty message.");
                return; // Ignore empty logs
            }

            // Ensure context is initialized
            if (string.IsNullOrEmpty(log.Context))
            {
                log = _logPool.GetLog(log.Level, "UnknownContext", log.Message, log.Stacktrace);
                clonedLog = log.Clone();
                _logPool.ReleaseLog(log);
            }

            CacheLog(clonedLog);

            RefreshListeners();
        }

        void HandleUnityLog(string logMessage, string stackTrace, LogType type)
        {
            if (Application.isPlaying)
            {
                VaultDebugLoggerMainThreadDispatcher.Instance().Enqueue(() =>
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
                LogType.Exception => LogLevel.Exception,
                LogType.Assert => LogLevel.Exception,
                _ => LogLevel.Info
            };

            // Filter compilation errors to avoid duplications
            if (logMessage.IsMatch(COMPILER_MESSAGE_PATTERN))
            {
                return;
            }

            var log = _logPool.GetLog(assignedLevel, "UNITY LOG", logMessage, stackTrace);
            var clonedLog = log.Clone();
            _logPool.ReleaseLog(log);

            CacheLog(clonedLog);

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

                // Pattern has 3 groups: Script path, code line and message
                var matchedGroups = compilerMessage.message.MatchOnce(COMPILER_MESSAGE_PATTERN);

                var path = matchedGroups[1].ToString();
                var line = matchedGroups[2].ToString();
                var message = matchedGroups[3].ToString();

                var log = _logPool.GetLog(LogLevel.Exception, "COMPILATION", message, $"{path}:{line}");
                var clonedLog = log.Clone();
                _logPool.ReleaseLog(log);

                CacheLog(clonedLog);
            }

            RefreshListeners();
        }

        public List<IVaultLog> GetLogsFiltered(string textFilter)
        {
            var filteredLogs = new List<IVaultLog>();

            // Extract @context filters from search query
            var contextFilters = new List<string>();
            if (!string.IsNullOrEmpty(textFilter))
            {
                var contextFilterMatches = Regex.Matches(textFilter, CONTEXT_FILTER_PATTERN);
                foreach (Match match in contextFilterMatches)
                {
                    var contextToFilter = match.Groups[1].Value;
                    contextFilters.Add(contextToFilter);
                }
            }

            // If filtering by context, use optimized lookup
            if (contextFilters.Count > 0)
            {
                foreach (var context in contextFilters)
                {
                    if (_logsByContext.ContainsKey(context))
                    {
                        filteredLogs.AddRange(_logsByContext[context]);
                    }
                }
            }
            else
            {
                // Otherwise, do standard level filtering
                foreach (var level in _logsByLevel.Keys)
                {
                    if (IsFilterActive(level))
                    {
                        filteredLogs.AddRange(_logsByLevel[level]);
                    }
                }
            }

            filteredLogs.Sort();

            return filteredLogs;
        }

        public IVaultLog GetLogWithId(long id)
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
            _logIdProvider.Reset();

            var allLogs = GetLogsFiltered(string.Empty);
            _logStorageService.SaveLogsAsync(allLogs);

            foreach (var level in _logsByLevel.Keys)
            {
                _logsByLevel[level].Clear();
            }

            _logsByContext.Clear();

            _logCount = 0;
            RefreshListeners();

        }

        void RefreshListeners()
        {
            foreach(var listener in _listeners)
            {
                listener.RefreshLogs();
            }
        }

        void CacheLog(IVaultLog log)
        {
            // Store in level-based dictionary (unchanged)
            _logsByLevel[log.Level].Add(log);

            // Store in context-based dictionary for faster filtering
            if (!_logsByContext.ContainsKey(log.Context))
            {
                _logsByContext[log.Context] = new List<IVaultLog>();
            }
            _logsByContext[log.Context].Add(log);

            _logCount++;

            EnforceMaxLogCount();
        }

        void EnforceMaxLogCount()
        {
            while (_logCount > MaxLogCached)
            {
                IVaultLog oldestLog = null;
                // Since _logsByLevel only has a few fixed keys (e.g., Info, Debug, Warn, Error, Exception),
                // we can iterate over each level’s list (which is ordered by insertion) and consider its first element.
                foreach (var logList in _logsByLevel.Values)
                {
                    if (logList.Count > 0)
                    {
                        var candidate = logList[0];
                        if (oldestLog == null || candidate.Id < oldestLog.Id)
                        {
                            oldestLog = candidate;
                        }
                    }
                }

                if (oldestLog != null)
                {
                    // Remove from the level-based dictionary
                    _logsByLevel[oldestLog.Level].Remove(oldestLog);

                    // Remove from the context-based dictionary
                    if (_logsByContext.TryGetValue(oldestLog.Context, out var contextLogs))
                    {
                        contextLogs.Remove(oldestLog);
                        if (contextLogs.Count == 0)
                        {
                            _logsByContext.Remove(oldestLog.Context);
                        }
                    }

                    _logCount--;
                }
                else
                {
                    break;
                }
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
            // By default all filters are active
            _activeFilters = (LogLevel)EditorPrefs.GetInt(Consts.EditorPrefKeys.ACTIVE_FILTERS_KEY, (int)(LogLevel.Debug | LogLevel.Error | LogLevel.Info | LogLevel.Warn |LogLevel.Exception));
        }

        void WritePreferences()
        {
            EditorPrefs.SetInt(Consts.EditorPrefKeys.ACTIVE_FILTERS_KEY, (int)_activeFilters);
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
                _logIdProvider.Reset();

                var allLogs = GetLogsFiltered(string.Empty);
                _logStorageService.SaveLogsAsync(allLogs);
                WritePreferences();

                _logDispatcher.UnregisterHandler(this);

                _logsByLevel.Clear();
                _logsByContext.Clear();
            }
        }

        #endregion
    }
}