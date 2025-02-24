using UnityEditor;
using UnityEngine;

namespace VaultDebug.Editor.Console
{
    class VaultConsoleSettings: EditorWindow
    {
        [MenuItem("Vault Debug/Settings")]
        public static void ShowWindow()
        {
            GetWindow<VaultConsoleSettings>("VaultDebug Settings");
        }

        [MenuItem("Vault Debug/Delete Editor Preferences")]
        public static void DeleteEditorPreferences()
        {
            EditorPrefs.DeleteKey(Consts.EditorPrefKeys.AUTO_OPEN_CONSOLE);
            EditorPrefs.DeleteKey(Consts.EditorPrefKeys.EXPORT_PATH);
            EditorPrefs.DeleteKey(Consts.EditorPrefKeys.ACTIVE_FILTERS_KEY);
        }

        void OnGUI()
        {
            GUILayout.Label("VaultDebug Configuration", EditorStyles.boldLabel);

            bool autoOpenConsole = EditorPrefs.GetBool(Consts.EditorPrefKeys.AUTO_OPEN_CONSOLE, true);
            string exportPath = EditorPrefs.GetString(Consts.EditorPrefKeys.EXPORT_PATH, Application.persistentDataPath);

            autoOpenConsole = EditorGUILayout.Toggle("Auto open Vault Console when opening unity", autoOpenConsole);
            exportPath = EditorGUILayout.TextField("Log Export Path", exportPath);

            EditorPrefs.SetBool(Consts.EditorPrefKeys.AUTO_OPEN_CONSOLE, autoOpenConsole);
            EditorPrefs.SetString(Consts.EditorPrefKeys.EXPORT_PATH, exportPath);
        }
    }
}
