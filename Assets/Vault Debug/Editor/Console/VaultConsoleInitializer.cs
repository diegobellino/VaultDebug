using UnityEditor;
using UnityEngine;
using VaultDebug.Editor.Console.Samples;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Editor.Console
{
    [InitializeOnLoad]
    class VaultConsoleInitializer
    {
        static VaultConsoleInitializer()
        {
            if (!EditorPrefs.GetBool("VaultDebug.AutoOpenConsole"))
            { 
                return; 
            }

            // Prevent opening a second window if the vault console is already opened
            if (Resources.FindObjectsOfTypeAll<VaultConsoleEditor>().Length > 0)
            {
                return;
            }

            VaultConsoleEditor.CreateWindow();
        }
    }
}
