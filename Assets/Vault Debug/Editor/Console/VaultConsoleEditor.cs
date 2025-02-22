using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using VaultDebug.Runtime.Logger;
using System.Text.RegularExpressions;
using UnityEditorInternal;

namespace VaultDebug.Editor.Console
{
    public class VaultConsoleEditor : EditorWindow, IVaultLogListener
    {
        const string STACKTRACE_PATTERN = @"(.*)(?:\s\(at\s)(.*):(\d*)";

        IVaultLogPool _logPool;
        IVaultLogDispatcher _logDispatcher;
        ILogStorageService _logStorageService;
        VaultEditorLogHandler _logHandler;

        Dictionary<LogLevel, Button> _filterButtons = new();
        TemplateContainer _visualTree;
        VisualElement _mainView;
        VisualElement _detailsView;
        VisualElement _focusedLogElement;
        ScrollView _logView;

        long _selectedLogId = -1;

        string _textFilter;

        [MenuItem("Vault Debug/Console/Open Window")]
        public static void CreateWindow()
        {
            var window = GetWindow(typeof(VaultConsoleEditor));
            window.minSize = new Vector2(700f, 110f);
            window.titleContent = new GUIContent("Vault Console");
        }

        void Initialize()
        {
            _logPool = DIBootstrapper.Container.Resolve<IVaultLogPool>();
            _logDispatcher = DIBootstrapper.Container.Resolve<IVaultLogDispatcher>();
            _logStorageService = new EditorFileLogStorageService();

            _logHandler = new VaultEditorLogHandler(_logPool, _logStorageService, _logDispatcher);
            _logHandler.Init();
            _logHandler.RegisterLogListener(this);
        }

        void CreateGUI()
        {
            Initialize();

            var root = rootVisualElement;
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VaultConsoleElements.MAIN_VIEW_PATH);

            if (asset == null)
            {
                Debug.LogError($"Failed to load UI asset: {VaultConsoleElements.MAIN_VIEW_PATH}");
                return;
            }

            _visualTree = asset.Instantiate();
            _visualTree.style.height = new StyleLength(Length.Percent(100));


            AddToolbarToTree();
            AddMainViewToTree();
            AddDetailsViewToTree();

            RefreshFilters();

            root.Add(_visualTree);
        }


        void OnDestroy()
        {
            _logHandler.UnregisterLogListener(this);
            _logHandler.Dispose();
        }

        // Executes 10 times per second
        void OnInspectorUpdate()
        {
            // If window is unfocused, auto scroll to bottom and disable detail view
            if (focusedWindow != this)
            {
                _logView.scrollOffset = Vector2.up * float.MaxValue;
            }
        }

        #region VIEWS

        void AddMainViewToTree()
        {
            _mainView = new VisualElement();
            _mainView.AddToClassList(VaultConsoleElements.MAIN_VIEW_CLASS_NAME);

            _logView  = new ScrollView();
            _logView.AddToClassList(VaultConsoleElements.LOG_VIEW_CLASS_NAME);

            _mainView.Add(_logView);
            _visualTree.Add(_mainView);
        }

        void AddDetailsViewToTree()
        {
            _detailsView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VaultConsoleElements.DETAILS_VIEW_PATH).Instantiate();
            
            var hideButton = _detailsView.Q<Button>(VaultConsoleElements.DETAILS_HIDE_BUTTON_CLASS_NAME);
            hideButton.clicked += () => {
                HideDetailsView();
            };

            HideDetailsView();
            _visualTree.Add(_detailsView);
        }

        void AddToolbarToTree()
        {
            _filterButtons.Clear();
            // Get parent element
            var toolbar = new VisualElement();
            toolbar.AddToClassList(VaultConsoleElements.TOOLBAR_CLASS_NAME);

            _filterButtons[LogLevel.Error] = CreateFilterButton(VaultConsoleElements.ERROR_BUTTON_CLASS_NAME, "E", () => { FilterLogLevel(LogLevel.Error); });
            _filterButtons[LogLevel.Warn] = CreateFilterButton(VaultConsoleElements.WARNING_BUTTON_CLASS_NAME, "W", () => { FilterLogLevel(LogLevel.Warn); });
            _filterButtons[LogLevel.Info] = CreateFilterButton(VaultConsoleElements.INFO_BUTTON_CLASS_NAME, "I", () => { FilterLogLevel(LogLevel.Info); } );
            _filterButtons[LogLevel.Debug] = CreateFilterButton(VaultConsoleElements.DEBUG_BUTTON_CLASS_NAME, "D", () => { FilterLogLevel(LogLevel.Debug); });

            var searchbar = new ToolbarSearchField();
            searchbar.AddToClassList(VaultConsoleElements.SEARCHBAR_CLASS_NAME);
            searchbar.name = "searchbar";
            searchbar.RegisterValueChangedCallback(OnFilterChanged);

            foreach(var button in _filterButtons.Values)
            {
                toolbar.Add(button);            
            }
            
            toolbar.Add(searchbar);

            var exportButton = new Button();
            exportButton.text = "Export";
            exportButton.RemoveFromClassList(VaultConsoleElements.UNITY_BUTTON_CLASS_NAME);
            exportButton.AddToClassList(VaultConsoleElements.TOOLBAR_BUTTON_CLASS_NAME);
            exportButton.clicked += () =>
            {
                var allLogs = _logHandler.GetLogsFiltered(string.Empty);
                _ = _logStorageService.ExportLogsAsync(allLogs, EditorPrefs.GetString(Consts.EditorPrefKeys.EXPORT_PATH));
            };
            toolbar.Add(exportButton);

            var clearButton = new Button();
            clearButton.text = "Clear";
            clearButton.RemoveFromClassList(VaultConsoleElements.UNITY_BUTTON_CLASS_NAME);
            clearButton.AddToClassList(VaultConsoleElements.TOOLBAR_BUTTON_CLASS_NAME);
            clearButton.clicked += ClearLogs;
            toolbar.Add(clearButton);

            _visualTree.Add(toolbar);
        }

        Button CreateFilterButton(string name, string label, Action onClick)
        {
            var button = new Button();
            button.RemoveFromClassList(VaultConsoleElements.UNITY_BUTTON_CLASS_NAME);
            button.AddToClassList(VaultConsoleElements.TOOLBAR_BUTTON_SMALL_CLASS_NAME);
            button.AddToClassList(VaultConsoleElements.TOOLBAR_BUTTON_CLASS_NAME);
            button.clicked += onClick;
            button.name = name;
            button.Add(new Label(label));

            return button;
        }

        void ShowDetailsView()
        {
            _detailsView.RemoveFromClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);

            var log = _logHandler.GetLogWithId(_selectedLogId);
            var timestampTag = _detailsView.Q<Label>(VaultConsoleElements.DETAILS_TIMESTAMP_TAG_NAME);
            timestampTag.text = new DateTime(log.TimeStampTicks).ToString("HH:mm:ss.ffff");

            var contextTag = _detailsView.Q<Label>(VaultConsoleElements.DETAILS_CONTEXT_TAG_NAME);
            contextTag.text = log.Context;

            var fullLog = _detailsView.Q<Label>(VaultConsoleElements.DETAILS_FULL_LOG_CLASS_NAME);
            fullLog.text = log.Message;
            fullLog.RemoveFromClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);

            var smartTab = _detailsView.Q<Button>(VaultConsoleElements.DETAILS_SMART_TAB_NAME);
            var smartContent = _detailsView.Q(VaultConsoleElements.DETAILS_SMART_STACKTRACE_NAME);

            var rawTab = _detailsView.Q<Button>(VaultConsoleElements.DETAILS_RAW_TAB_NAME);
            var rawContent = _detailsView.Q(VaultConsoleElements.DETAILS_RAW_STACKTRACE_NAME);

            smartTab.clicked += () => SelectSmartTab();
            rawTab.clicked += () => SelectRawTab();

            // Smart stacktrace is selected by default
            SelectSmartTab();

            void SelectSmartTab()
            {
                var matchCollection = log.Stacktrace.MatchAll(STACKTRACE_PATTERN);

                smartContent.Clear();

                if (matchCollection.Count > 0)
                {
                    foreach (Match match in matchCollection)
                    {
                        var method = match.Groups[1].ToString();
                        var path = match.Groups[2].ToString();
                        var line = int.Parse(match.Groups[3].ToString());

                        var stackTrace = CreateURLText($"{method} at line {line}",
                            () => InternalEditorUtility.OpenFileAtLineExternal(path, line));

                        stackTrace.AddToClassList("stacktrace-element");
                        smartContent.Add(stackTrace);
                    }
                }
                else
                {
                    // Handle compilation logs (no method context)
                    string[] stackParts = log.Stacktrace.Split(':');
                    if (stackParts.Length == 2)
                    {
                        string path = stackParts[0];
                        if (int.TryParse(stackParts[1], out int line))
                        {
                            var stackTrace = CreateURLText($"Compilation Error at {path} (line {line})",
                                () => InternalEditorUtility.OpenFileAtLineExternal(path, line));

                            stackTrace.AddToClassList("stacktrace-element");
                            smartContent.Add(stackTrace);
                        }
                    }
                }

                rawTab.RemoveFromClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
                rawContent.AddToClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);

                smartTab.AddToClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
                smartContent.RemoveFromClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);
            }

            void SelectRawTab()
            {
                rawContent.Clear();
                
                var textElement = new TextElement();
                textElement.text = log.Stacktrace;

                rawContent.Add(textElement);

                rawTab.AddToClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
                rawContent.RemoveFromClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);

                smartTab.RemoveFromClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
                smartContent.AddToClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);
            }
        }

        void HideDetailsView()
        {
            _detailsView.AddToClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);

            _focusedLogElement?.RemoveFromClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
            _focusedLogElement = null;
            _selectedLogId = -1;

            var stacktrace = _detailsView.Q(VaultConsoleElements.DETAILS_SMART_STACKTRACE_NAME);
            stacktrace.AddToClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);

            var fullLog = _detailsView.Q<Label>(VaultConsoleElements.DETAILS_FULL_LOG_CLASS_NAME);
            fullLog.AddToClassList(VaultConsoleElements.HIDDEN_ELEMENT_CLASS_NAME);
        }

        VisualElement CreateURLText(string text, Action OnClick)
        {
            var urlElement = new TextElement();
            urlElement.text = $"- <color=#5D93BC>{text}</color>";
            urlElement.AddManipulator(new Clickable(OnClick));

            return urlElement;
        }

        VisualElement CreateLogVisualElement(IVaultLog log, long id, bool isEven)
        {
            var logElement = new VisualElement();

            logElement.AddToClassList(VaultConsoleElements.LOG_ELEMENT_CLASS_NAME);

            if (log.Level == LogLevel.Exception)
            {
                logElement.AddToClassList(VaultConsoleElements.LOG_ELEMENT_CRITICAL_CLASS_NAME);
            }
            else if (isEven)
            {
                logElement.AddToClassList(VaultConsoleElements.LOG_ELEMENT_EVEN_CLASS_NAME);
            }

            var logIconClass = log.Level switch
            {
                LogLevel.Info => VaultConsoleElements.INFO_ICON_CLASS_NAME,
                LogLevel.Debug => VaultConsoleElements.DEBUG_ICON_CLASS_NAME,
                LogLevel.Warn => VaultConsoleElements.WARNING_ICON_CLASS_NAME,
                LogLevel.Error or LogLevel.Exception => VaultConsoleElements.ERROR_ICON_CLASS_NAME,
                _ => VaultConsoleElements.INFO_ICON_CLASS_NAME
            };

            var logIconLabel = log.Level switch
            {
                LogLevel.Info => new Label("I"),
                LogLevel.Debug => new Label("D"),
                LogLevel.Warn => new Label("W"),
                LogLevel.Error or LogLevel.Exception => new Label("E"),
                _ => new Label("I")
            };
            logIconLabel.AddToClassList(VaultConsoleElements.LOG_ICON_CLASS_NAME);
            logIconLabel.AddToClassList(logIconClass);

            var logMessageLabel = new Label(log.Message);
            logMessageLabel.AddToClassList(VaultConsoleElements.LOG_TEXT_CLASS_NAME);

            var contextTag = new Label(log.Context);
            contextTag.AddToClassList(VaultConsoleElements.LOG_TAG_CLASS_NAME);

            if (id == _selectedLogId)
            {
                logElement.AddToClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
                _focusedLogElement = logElement;
            }

            logElement.Add(logIconLabel);
            logElement.Add(logMessageLabel);
            logElement.Add(contextTag);
            logElement.AddManipulator(
                new Clickable(() =>
                {
                    if (_focusedLogElement != null)
                    {
                        _focusedLogElement.RemoveFromClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
                    }

                    _selectedLogId = id;
                    _focusedLogElement = logElement;
                    _focusedLogElement.AddToClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);

                    OnLogSelected(log);
                }));

            return logElement;
        }

        void OnLogSelected(IVaultLog log)
        {
            var stackTrace = log.Stacktrace;

            ShowDetailsView();

            // delayCall executes after all inspectors have been updated. Must be delayed to let detailsView accomodate to new height before scrolling
            // to element
            EditorApplication.delayCall += () =>
            {
                _logView.ScrollTo(_focusedLogElement);
            };
        }

        #endregion

        #region FILTERING

        void OnFilterChanged(ChangeEvent<string> changeEvent)
        {
            _textFilter = changeEvent.newValue;
            RefreshLogs();
        }

        void FilterLogLevel(LogLevel logLevel)
        {
            if (!_filterButtons.ContainsKey(logLevel) || _filterButtons[logLevel] == null)
            {
                throw new KeyNotFoundException($"Log level {logLevel} not found or button is null");
            }

            _logHandler.TriggerFilter(logLevel);
            RefreshFilters();
        }

        void RefreshFilters()
        {
            foreach(var level in _filterButtons.Keys)
            {
                if (_logHandler.IsFilterActive(level))
                {
                    _filterButtons[level].AddToClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
                }
                else
                {
                    _filterButtons[level].RemoveFromClassList(VaultConsoleElements.ACTIVE_ELEMENT_CLASS_NAME);
                }
            }

            RefreshLogs();
        }

        #endregion

        #region LOGGING

        public void ClearLogs()
        {
            _logHandler.ClearLogs();
            RefreshLogs();
        }

        public void RefreshLogs()
        {
            var logContainer = _mainView.Q(classes: VaultConsoleElements.LOG_VIEW_CLASS_NAME);

            var existingLogs = logContainer.childCount;
            var filteredLogs = _logHandler.GetLogsFiltered(_textFilter);

            if (filteredLogs.Count == existingLogs)
            {
                return;
            }

            if (filteredLogs.Count <= 0)
            {
                logContainer.Clear();
                return;
            }

            filteredLogs.Sort();

            for (int i = existingLogs; i < filteredLogs.Count; i++)
            {
                var log = filteredLogs[i];
                var logElement = CreateLogVisualElement(log, log.Id, i % 2 == 0);

                logContainer.Add(logElement);
            }
        }

        #endregion

    }
}