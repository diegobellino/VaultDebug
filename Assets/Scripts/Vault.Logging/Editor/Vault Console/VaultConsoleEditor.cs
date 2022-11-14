using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using Vault.Logging.Runtime;
using System.Collections.Generic;

namespace Vault.Logging.Editor.VaultConsole
{
    public class VaultConsoleEditor : EditorWindow, IVaultLogListener
    {
        #region VARIABLES

        const string BASE_PATH = "Assets/Scripts/Vault.Logging/Editor/Vault Console/";

        const string UNITY_BUTTON_CLASS_NAME = "unity-button";

        const string ACTIVE_ELEMENT_CLASS_NAME = "active";
        const string HIDE_ELEMENT_CLASS_NAME = "hidden";

        const string TOOLBAR_CLASS_NAME = "toolbar";
        const string TOOLBAR_BUTTON_CLASS_NAME = "toolbar-button";

        const string ERROR_BUTTON_CLASS_NAME = "error-button";
        const string WARNING_BUTTON_CLASS_NAME = "warning-button";
        const string DEBUG_BUTTON_CLASS_NAME = "debug-button";
        const string INFO_BUTTON_CLASS_NAME = "info-button";
        const string HIDE_BUTTON_CLASS_NAME = "hide-button";

        const string SEARCHBAR_CLASS_NAME = "searchbar";

        const string LOG_VIEW_CLASS_NAME = "log-view";
        const string LOG_ELEMENT_CLASS_NAME = "log-element";
        const string LOG_ELEMENT_EVEN_CLASS_NAME = "log-element-even";
        const string LOG_ELEMENT_CRITICAL_CLASS_NAME = "log-element-critical";
        const string LOG_TEXT_CLASS_NAME = "log-text";
        const string LOG_ICON_CLASS_NAME = "log-icon";
        
        const string MAIN_VIEW_CLASS_NAME = "main-view";
        const string DETAILS_VIEW_CLASS_NAME = "details-view";

        const string ERROR_ICON_CLASS_NAME = "error-icon";
        const string WARNING_ICON_CLASS_NAME = "warning-icon";
        const string DEBUG_ICON_CLASS_NAME = "debug-icon";
        const string INFO_ICON_CLASS_NAME = "info-icon";

        const string STACKTRACE_PATTERN = "[^\\n|\"].*?(?=\\n)";

        Dictionary<LogLevel, Button> _filterButtons = new();
        TemplateContainer _visualTree;
        VisualElement _mainView;
        VisualElement _detailsView;

        VaultConsoleLogHandler _logHandler = new();

        static VaultLogger Logger = VaultLoggerFactory.GetOrCreateLogger("VAULT CONSOLE");

        VisualElement _focusedLog;
        ScrollView _logView;

        int count = 0;

        #endregion

        [MenuItem("Vault/Vault Console")]
        public static void CreateWindow()
        {
            var window = GetWindow<VaultConsoleEditor>();
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
            if (count < 5)
            {
                count++;
            }
            else
            {
                count = 0;
                Logger.Info("This is an info log");
            }

            if (focusedWindow != this)
            {
                _logView.scrollOffset = Vector2.up * float.MaxValue;
            }

            Repaint();
        }

        #region VIEWS

        void AddMainViewToTree()
        {
            _mainView = new VisualElement();
            _mainView.AddToClassList(MAIN_VIEW_CLASS_NAME);

            _logView  = new ScrollView();
            _logView.AddToClassList(LOG_VIEW_CLASS_NAME);

            _mainView.Add(_logView);
            _visualTree.Add(_mainView);
        }

        void AddDetailsViewToTree()
        {
            _detailsView = new VisualElement();
            _detailsView.AddToClassList(DETAILS_VIEW_CLASS_NAME);

            TriggerDetailsViewVisibility(false);

            var hideButton = new Button(() =>
            {
                TriggerDetailsViewVisibility(false);
            });
            hideButton.AddToClassList(HIDE_BUTTON_CLASS_NAME);

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
            toolbar.AddToClassList(TOOLBAR_CLASS_NAME);

            _filterButtons[LogLevel.Error] = CreateToolbarButton(ERROR_BUTTON_CLASS_NAME, "E", () => { FilterLogLevel(LogLevel.Error); });
            _filterButtons[LogLevel.Warn] = CreateToolbarButton(WARNING_BUTTON_CLASS_NAME, "W", () => { FilterLogLevel(LogLevel.Warn); });
            _filterButtons[LogLevel.Debug] = CreateToolbarButton(DEBUG_BUTTON_CLASS_NAME, "D", () => { FilterLogLevel(LogLevel.Debug); });
            _filterButtons[LogLevel.Info] = CreateToolbarButton(INFO_BUTTON_CLASS_NAME, "I", () => { FilterLogLevel(LogLevel.Info); });

            var searchbar = new ToolbarSearchField();
            searchbar.AddToClassList(SEARCHBAR_CLASS_NAME);
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
            button.RemoveFromClassList(UNITY_BUTTON_CLASS_NAME);
            button.AddToClassList(TOOLBAR_BUTTON_CLASS_NAME);
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
                _focusedLog?.RemoveFromClassList(ACTIVE_ELEMENT_CLASS_NAME);
                _focusedLog = null;
            }
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
                    _filterButtons[level].AddToClassList(ACTIVE_ELEMENT_CLASS_NAME);
                }
                else
                {
                    _filterButtons[level].RemoveFromClassList(ACTIVE_ELEMENT_CLASS_NAME);
                }
            }

            RefreshLogs();
        }

        #endregion

        #region LOGGING

        public void RefreshLogs()
        {
            var logContainer = _mainView.Q(classes: LOG_VIEW_CLASS_NAME);
            logContainer.Clear();

            var isEven = false;
            var filteredLogs = _logHandler.GetLogsFiltered();

            foreach (var log in filteredLogs)
            {
                var logElement = CreateLogVisualElement(log, isEven);

                logContainer.Add(logElement);

                isEven = !isEven;
            }
        }

        VisualElement CreateLogVisualElement(VaultLog log, bool isEven)
        {
            var logElement = new VisualElement();

            logElement.AddToClassList(LOG_ELEMENT_CLASS_NAME);

            if (log.Level == LogLevel.Exception)
            {
                logElement.AddToClassList(LOG_ELEMENT_CRITICAL_CLASS_NAME);
            }
            else if (isEven)
            {
                logElement.AddToClassList(LOG_ELEMENT_EVEN_CLASS_NAME);
            }

            var logIconClass = log.Level switch
            {
                LogLevel.Info => INFO_ICON_CLASS_NAME,
                LogLevel.Debug => DEBUG_ICON_CLASS_NAME,
                LogLevel.Warn => WARNING_ICON_CLASS_NAME,
                LogLevel.Error or LogLevel.Exception => ERROR_ICON_CLASS_NAME,
                _ => INFO_ICON_CLASS_NAME
            };

            var logIconLabel = log.Level switch
            {
                LogLevel.Info => new Label("I"),
                LogLevel.Debug => new Label("D"),
                LogLevel.Warn => new Label("W"),
                LogLevel.Error or LogLevel.Exception => new Label("E"),
                _ => new Label("I")
            };
            logIconLabel.AddToClassList(LOG_ICON_CLASS_NAME);
            logIconLabel.AddToClassList(logIconClass);

            logElement.Add(logIconLabel);

            var logMessageLabel = new Label($"[{log.Context}] {log.Message}");
            logMessageLabel.AddToClassList(LOG_TEXT_CLASS_NAME);

            logElement.Add(logMessageLabel);
            logElement.AddManipulator(
                new Clickable(() => 
                {
                    if (_focusedLog != null)
                    {
                        _focusedLog.RemoveFromClassList(ACTIVE_ELEMENT_CLASS_NAME);
                    }

                    _focusedLog = logElement;
                    _focusedLog.AddToClassList(ACTIVE_ELEMENT_CLASS_NAME);

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
                _logView.ScrollTo(_focusedLog);
            };            
        }

        #endregion

    }
}