using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using Elements = VaultDebug.Console.Editor.VaultConsoleElements;
using VaultDebug.Logging.Runtime;

namespace VaultDebug.Console.Editor
{
    public class VaultConsoleEditor : EditorWindow, IVaultLogListener
    {
        #region VARIABLES

        const string BASE_PATH = "Assets/Scripts/Vault Debug/Console/Editor/";
        const string STACKTRACE_PATTERN = "[^\\n|\"].*?(?=\\n)";

        Dictionary<LogLevel, Button> _filterButtons = new();
        TemplateContainer _visualTree;
        VisualElement _mainView;
        VisualElement _detailsView;

        readonly VaultConsoleLogHandler _logHandler = new();

        static VaultLogger Logger = VaultLoggerFactory.GetOrCreateLogger("VAULT CONSOLE");

        VisualElement _focusedLogElement;
        ScrollView _logView;

        int _count = 0;
        int _selectedLogId = -1;

        #endregion

        [MenuItem("Vault/Vault Console")]
        public static void CreateWindow()
        {
            var window = GetWindow(typeof(VaultConsoleEditor));
            window.titleContent = new GUIContent("Vault Console");
        }
    
        void CreateGUI()
        {
            _logHandler.Init();
            _logHandler.RegisterListener(this);
            
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(BASE_PATH + "VaultConsoleEditor.uxml");
            _visualTree = visualTree.Instantiate();
            _visualTree.style.height = new StyleLength(Length.Percent(100));

            AddToolbarToTree();
            AddMainViewToTree();
            AddDetailsViewToTree();

            RefreshFilters();

            root.Add(_visualTree);
        }

        void OnDestroy()
        {
            _logHandler.UnregisterListener(this);
            _logHandler.Dispose();
        }

        // Executes 10 times per second
        void OnInspectorUpdate()
        {
            if (_count < 10)
            {
                _count++;
            }
            else
            {
                _count = 0;
                Logger.Info("This is an info log");
            }

            // If window is unfocused, auto scroll to bottom and disable detail view
            if (focusedWindow != this)
            {
                _logView.scrollOffset = Vector2.up * float.MaxValue;
                TriggerDetailsViewVisibility(false);
            }
        }

        #region VIEW

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
            _detailsView = new VisualElement();
            _detailsView.AddToClassList(Elements.DETAILS_VIEW_CLASS_NAME);

            TriggerDetailsViewVisibility(false);

            var hideButton = new Button(() =>
            {
                TriggerDetailsViewVisibility(false);
            });
            hideButton.AddToClassList(Elements.HIDE_BUTTON_CLASS_NAME);

            // Unicode char for down triangle
            var downArrow = '\u25BC';
            var hideLabel = new Label(downArrow.ToString());

            hideButton.Add(hideLabel);
            _detailsView.Add(hideButton);
            _visualTree.Add(_detailsView);
        }

        void AddToolbarToTree()
        {
            _filterButtons.Clear();
            // Get parent element
            var toolbar = new VisualElement();
            toolbar.AddToClassList(Elements.TOOLBAR_CLASS_NAME);

            _filterButtons[LogLevel.Error] = CreateToolbarButton(Elements.ERROR_BUTTON_CLASS_NAME, "E", () => { FilterLogLevel(LogLevel.Error); });
            _filterButtons[LogLevel.Warn] = CreateToolbarButton(Elements.WARNING_BUTTON_CLASS_NAME, "W", () => { FilterLogLevel(LogLevel.Warn); });
            _filterButtons[LogLevel.Debug] = CreateToolbarButton(Elements.DEBUG_BUTTON_CLASS_NAME, "D", () => { FilterLogLevel(LogLevel.Debug); });
            _filterButtons[LogLevel.Info] = CreateToolbarButton(Elements.INFO_BUTTON_CLASS_NAME, "I", () => { FilterLogLevel(LogLevel.Info); });

            var searchbar = new ToolbarSearchField();
            searchbar.AddToClassList(Elements.SEARCHBAR_CLASS_NAME);
            searchbar.name = "searchbar";

            foreach(var button in _filterButtons.Values)
            {
                toolbar.Add(button);            
            }
            
            toolbar.Add(searchbar);

            _visualTree.Add(toolbar);
        }

        Button CreateToolbarButton(string name, string label, Action onClick)
        {
            var button = new Button();
            button.RemoveFromClassList(Elements.UNITY_BUTTON_CLASS_NAME);
            button.AddToClassList(Elements.TOOLBAR_BUTTON_CLASS_NAME);
            button.clicked += onClick;
            button.name = name;
            button.Add(new Label(label));

            return button;
        }

        void TriggerDetailsViewVisibility(bool isOn)
        {
            _detailsView.style.minHeight = isOn ? new Length(60, LengthUnit.Percent) : 0;
            _detailsView.style.visibility = isOn ? Visibility.Visible : Visibility.Hidden;

            if (!isOn)
            {
                _focusedLogElement?.RemoveFromClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
                _focusedLogElement = null;
                _selectedLogId = -1;
            }
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

            var logMessageLabel = new Label($"[{log.Context}] {log.Message}");
            logMessageLabel.AddToClassList(Elements.LOG_TEXT_CLASS_NAME);

            if (id == _selectedLogId)
            {
                logElement.AddToClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);
                _focusedLogElement = logElement;
            }

            logElement.Add(logIconLabel);
            logElement.Add(logMessageLabel);
            logElement.AddManipulator(
                new Clickable(() =>
                {
                    _selectedLogId = id;
                    _focusedLogElement = logElement;
                    _focusedLogElement.AddToClassList(Elements.ACTIVE_ELEMENT_CLASS_NAME);

                    OnLogSelected(log);
                }));

            return logElement;
        }

        void OnLogSelected(VaultLog log)
        {
            var stackTrace = log.StackTrace;

            TriggerDetailsViewVisibility(true);

            // delayCall executes after all inspectors have been updated. Must be delayed to let detailsView accomodate to new height before scrolling
            // to element
            EditorApplication.delayCall += () =>
            {
                _logView.ScrollTo(_focusedLogElement);
            };
        }

        #endregion

        #region FILTERING

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

        public void RefreshLogs()
        {
            var logContainer = _mainView.Q(classes: Elements.LOG_VIEW_CLASS_NAME);
            logContainer.Clear();

            var isEven = false;
            var filteredLogs = _logHandler.GetLogsFiltered();

            foreach (var kvp in filteredLogs)
            {
                var id = kvp.Key;
                var log = kvp.Value;

                var logElement = CreateLogVisualElement(log, id, isEven);

                logContainer.Add(logElement);

                isEven = !isEven;
            }
        }

        #endregion

    }
}