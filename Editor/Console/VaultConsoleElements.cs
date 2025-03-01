using System.IO;
using UnityEditor;
using UnityEngine;

namespace VaultDebug.Editor.Console
{
    public static class VaultConsoleElements
    {
        private static string PackagePath => GetPackagePath();
        public static string MAIN_VIEW_PATH => Path.Combine(PackagePath, "UI/VaultConsoleMainView.uxml");
        public static string DETAILS_VIEW_PATH => Path.Combine(PackagePath, "UI/VaultConsoleDetailsView.uxml");


        private static string GetPackagePath()
        {
            // Finds the path dynamically
            string[] results = AssetDatabase.FindAssets("VaultConsoleElements");
            if (results.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(results[0]);
                return Path.GetDirectoryName(assetPath).Replace("\\", "/"); // Normalize path
            }

            Debug.LogError("VaultDebug package path could not be found!");
            return "Packages/com.vaultdebug.logging"; // Default fallback
        }

        #region MAIN VIEW

        public const string UNITY_BUTTON_CLASS_NAME = "unity-button";

        public const string ACTIVE_ELEMENT_CLASS_NAME = "active";
        public const string HIDDEN_ELEMENT_CLASS_NAME = "hidden";
        public const string MINIMIZED_VIEW_CLASS_NAME = "minimized-view";

        public const string TOOLBAR_CLASS_NAME = "toolbar";
        public const string TOOLBAR_BUTTON_CLASS_NAME = "toolbar-button";
        public const string TOOLBAR_BUTTON_SMALL_CLASS_NAME = "toolbar-button-small";
        public const string TOOLBAR_BUTTON_FIRST_CLASS_NAME = "toolbar-button-first";

        public const string ERROR_BUTTON_CLASS_NAME = "error-button";
        public const string WARNING_BUTTON_CLASS_NAME = "warning-button";
        public const string DEBUG_BUTTON_CLASS_NAME = "debug-button";
        public const string INFO_BUTTON_CLASS_NAME = "info-button";

        public const string SEARCHBAR_CLASS_NAME = "searchbar";

        public const string LOG_VIEW_CLASS_NAME = "log-view";
        public const string LOG_ELEMENT_CLASS_NAME = "log-element";
        public const string LOG_ELEMENT_EVEN_CLASS_NAME = "log-element-even";
        public const string LOG_ELEMENT_CRITICAL_CLASS_NAME = "log-element-critical";
        public const string LOG_TEXT_CLASS_NAME = "log-text";
        public const string LOG_ICON_CLASS_NAME = "log-icon";
        public const string LOG_TAG_CLASS_NAME = "log-tag";

        public const string MAIN_VIEW_CLASS_NAME = "main-view";

        public const string ERROR_ICON_CLASS_NAME = "error-icon";
        public const string WARNING_ICON_CLASS_NAME = "warning-icon";
        public const string DEBUG_ICON_CLASS_NAME = "debug-icon";
        public const string INFO_ICON_CLASS_NAME = "info-icon";

        #endregion

        #region DETAILS VIEW

        public const string DETAILS_HIDE_BUTTON_CLASS_NAME = "hide-button";
        public const string DETAILS_TIMESTAMP_TAG_NAME = "timestamp-tag";
        public const string DETAILS_CONTEXT_TAG_NAME = "context-tag";

        public const string DETAILS_SMART_TAB_NAME = "smart-tab";
        public const string DETAILS_RAW_TAB_NAME = "raw-tab";
        public const string DETAILS_SMART_STACKTRACE_NAME = "smart-content";
        public const string DETAILS_RAW_STACKTRACE_NAME = "raw-content";

        public const string DETAILS_FULL_LOG_CLASS_NAME = "full-log";
        public const string DETAILS_PROPERTIES_CLASS_NAME = "property-list";

        #endregion

    }
}
