using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using Elements = VaultDebug.Console.Editor.VaultConsoleElements;
using VaultDebug.Logging.Runtime;
using System.Text.RegularExpressions;
using UnityEditorInternal;

namespace VaultDebug.Console.Editor
{
    public class VaultConsoleEditor : EditorWindow, IVaultLogListener
    {
        #region VARIABLES

        const string STACKTRACE_PATTERN = @"(.*)(?:\s\(at\s)(.*):(\d*)";

        Dictionary<LogLevel, Button> _filterButtons = new();
        TemplateContainer _visualTree;
        VisualElement _mainView;
        VisualElement _detailsView;

        readonly VaultConsoleLogHandler _logHandler = new();

        VisualElement _focusedLogElement;
        ScrollView _logView;

        int _selectedLogId = -1;

        string _textFilter;

        #endregion

        [MenuItem("Vault Console/Console Window")]
        public static void CreateWindow()
        {
            var window = GetWindow(typeof(VaultConsoleEditor));
            window.titleContent = new GUIContent("Vault Console");
        }

        [MenuItem("Vault Console/Debug/Generate test logs")]
        public static void TestLogs()
        {
            VaultDebugLoggerInternal.Logger.Debug("Long debug log from internal logger - Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce at dignissim odio. Suspendisse sed consequat justo. Phasellus consequat, est vitae auctor mollis, mi nunc volutpat tortor, sed auctor magna dui vitae nulla. Curabitur eu tincidunt dui. Donec condimentum libero sit amet magna rhoncus, eu tristique sapien vestibulum. Phasellus volutpat, eros at auctor placerat, ipsum felis venenatis velit, eget mattis turpis tortor vel diam. Nulla eu mauris eu libero congue rhoncus ac sed nunc. Duis maximus ultrices elit, in varius ipsum sodales in. Aenean nisl erat, porttitor nec laoreet non, placerat dignissim enim. ");
            VaultDebugLoggerSample.Logger.Error("Error log from another logger");
            VaultDebugLoggerInternal.Logger.Warn("Warn log from internal logger");
            VaultDebugLoggerSample.Logger.Info("Info log from another logger");
        }

        void CreateGUI()
        {
            _logHandler.Init();
            _logHandler.RegisterLogListener(this);
            
            var root = rootVisualElement;
            _visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Elements.MAIN_VIEW_PATH).Instantiate();
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
            _mainView.AddToClassList(Elements.MAIN_VIEW_CLASS_NAME);

            _logView  = new ScrollView();
            _logView.AddToClassList(Elements.LOG_VIEW_CLASS_NAME);

            _mainView.Add(_logView);
            _visualTree.Add(_mainView);
        }

        void AddDetailsViewToTree()
        {
            _detailsView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Elements.DETAILS_VIEW_PATH).Instantiate();
            
            var hideButton = _detailsView.Q<Button>(Elements.DETAILS_HIDE_BUTTON_CLASS_NAME);
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
            toolbar.AddToClassList(Elements.TOOLBAR_CLASS_NAME);

            _filterButtons[LogLevel.Error] = CreateFilterButton(Elements.ERROR_BUTTON_CLASS_NAME, "E", () => { FilterLogLevel(LogLevel.Error); });
            _filterButtons[LogLevel.Warn] = CreateFilterButton(Elements.WARNING_BUTTON_CLASS_NAME, "W", () => { FilterLogLevel(LogLevel.Warn); });
            _filterButtons[LogLevel.Info] = CreateFilterButton(Elements.INFO_BUTTON_CLASS_NAME, "I", () => { FilterLogLevel(LogLevel.Info); } );
            _filterButtons[LogLevel.Debug] = CreateFilterButton(Elements.DEBUG_BUTTON_CLASS_NAME, "D", () => { FilterLogLevel(LogLevel.Debug); });

            var searchbar = new ToolbarSearchField();
            searchbar.AddToClassList(Elements.SEARCHBAR_CLASS_NAME);
            searchbar.name = "searchbar";
            searchbar.RegisterValueChangedCallback(OnFilterChanged);

            foreach(var button in _filterButtons.Values)
            {
                toolbar.Add(button);            
            }
            
            toolbar.Add(searchbar);

            var exportButton = new Button();
            exportButton.text = "Export";
            exportButton.RemoveFromClassList(Elements.UNITY_BUTTON_CLASS_NAME);
            exportButton.AddToClassList(Elements.TOOLBAR_BUTTON_CLASS_NAME);
            exportButton.clicked += _logHandler.ExportLogs;
            toolbar.Add(exportButton);

            var clearButton = new Button();
            clearButton.text = "Clear";
            clearButton.RemoveFromClassList(Elements.UNITY_BUTTON_CLASS_NAME);
            clearButton.AddToClassList(Elements.TOOLBAR_BUTTON_CLASS_NAME);
            clearButton.clicked += ClearLogs;
            toolbar.Add(clearButton);

            _visualTree.Add(toolbar);
        }

        Button CreateFilterButton(string name, string label, Action onClick)
        {
            var button = new Button();
            button.RemoveFromClassList(Elements.UNITY_BUTTON_CLASS_NAME);
            button.AddToClassList(Elements.TOOLBAR_BUTTON_SMALL_CLASS_NAME);
            button.AddToClassList(Elements.TOOLBAR_BUTTON_CLASS_NAME);
            button.clicked += onClick;
            button.name = name;
            button.Add(new Label(label));

            return button;
        }

        void ShowDetailsView()
        {
            _detailsView.RemoveFromClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);

            var log = _logHandler.GetLogWithId(_selectedLogId);
            var timestampTag = _detailsView.Q<Label>(Elements.DETAILS_TIMESTAMP_TAG_NAME);
            timestampTag.text = log.TimeStamp;

            var contextTag = _detailsView.Q<Label>(Elements.DETAILS_CONTEXT_TAG_NAME);
            contextTag.text = log.Context;

            var fullLog = _detailsView.Q<Label>(Elements.DETAILS_FULL_LOG_CLASS_NAME);
            fullLog.text = log.Message;
            fullLog.RemoveFromClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);

            var smartTab = _detailsView.Q<Button>(Elements.DETAILS_SMART_TAB_NAME);
            var smartContent = _detailsView.Q(Elements.DETAILS_SMART_STACKTRACE_NAME);

            var rawTab = _detailsView.Q<Button>(Elements.DETAILS_RAW_TAB_NAME);
            var rawContent = _detailsView.Q(Elements.DETAILS_RAW_STACKTRACE_NAME);

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

                rawTab.RemoveFromClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
                rawContent.AddToClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);

                smartTab.AddToClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
                smartContent.RemoveFromClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);
            }

            void SelectRawTab()
            {
                rawContent.Clear();
                
                var textElement = new TextElement();
                textElement.text = log.Stacktrace;

                rawContent.Add(textElement);

                rawTab.AddToClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
                rawContent.RemoveFromClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);

                smartTab.RemoveFromClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
                smartContent.AddToClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);
            }
        }

        void HideDetailsView()
        {
            _detailsView.AddToClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);

            _focusedLogElement?.RemoveFromClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
            _focusedLogElement = null;
            _selectedLogId = -1;

            var stacktrace = _detailsView.Q(Elements.DETAILS_SMART_STACKTRACE_NAME);
            stacktrace.AddToClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);

            var fullLog = _detailsView.Q<Label>(Elements.DETAILS_FULL_LOG_CLASS_NAME);
            fullLog.AddToClassList(Elements.HIDDEN_ELEMENT_CLASS_NAME);
        }

        VisualElement CreateURLText(string text, Action OnClick)
        {
            var urlElement = new TextElement();
            urlElement.text = $"- <color=#5D93BC>{text}</color>";
            urlElement.AddManipulator(new Clickable(OnClick));

            return urlElement;
        }

        VisualElement CreateLogVisualElement(VaultLog log, int id, bool isEven)
        {
            var logElement = new VisualElement();

            logElement.AddToClassList(Elements.LOG_ELEMENT_CLASS_NAME);

            if (log.Level == LogLevel.Exception)
            {
                logElement.AddToClassList(Elements.LOG_ELEMENT_CRITICAL_CLASS_NAME);
            }
            else if (isEven)
            {
                logElement.AddToClassList(Elements.LOG_ELEMENT_EVEN_CLASS_NAME);
            }

            var logIconClass = log.Level switch
            {
                LogLevel.Info => Elements.INFO_ICON_CLASS_NAME,
                LogLevel.Debug => Elements.DEBUG_ICON_CLASS_NAME,
                LogLevel.Warn => Elements.WARNING_ICON_CLASS_NAME,
                LogLevel.Error or LogLevel.Exception => Elements.ERROR_ICON_CLASS_NAME,
                _ => Elements.INFO_ICON_CLASS_NAME
            };

            var logIconLabel = log.Level switch
            {
                LogLevel.Info => new Label("I"),
                LogLevel.Debug => new Label("D"),
                LogLevel.Warn => new Label("W"),
                LogLevel.Error or LogLevel.Exception => new Label("E"),
                _ => new Label("I")
            };
            logIconLabel.AddToClassList(Elements.LOG_ICON_CLASS_NAME);
            logIconLabel.AddToClassList(logIconClass);

            var logMessageLabel = new Label(log.Message);
            logMessageLabel.AddToClassList(Elements.LOG_TEXT_CLASS_NAME);

            var contextTag = new Label(log.Context);
            contextTag.AddToClassList(Elements.LOG_TAG_CLASS_NAME);

            if (id == _selectedLogId)
            {
                logElement.AddToClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
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
                        _focusedLogElement.RemoveFromClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
                    }

                    _selectedLogId = id;
                    _focusedLogElement = logElement;
                    _focusedLogElement.AddToClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);

                    OnLogSelected(log);
                }));

            return logElement;
        }

        void OnLogSelected(VaultLog log)
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
                    _filterButtons[level].AddToClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
                }
                else
                {
                    _filterButtons[level].RemoveFromClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
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
            var logContainer = _mainView.Q(classes: Elements.LOG_VIEW_CLASS_NAME);

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