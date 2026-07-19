using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Writes Vault logs to the Unity Console.
    /// </summary>
    public sealed class UnityConsoleLogHandler : IVaultLogHandler
    {
        public int MaxLogCached => 0;

        public void RegisterLogListener(IVaultLogListener listener)
        {
        }

        public void UnregisterLogListener(IVaultLogListener listener)
        {
        }

        public void HandleLog(IVaultLog log)
        {
            if (log == null)
            {
                return;
            }

            var message = FormatMessage(log);

            switch (log.Level)
            {
                case LogLevel.Warn:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                case LogLevel.Exception:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }

        static string FormatMessage(IVaultLog log)
        {
            var contextTag = $"[{log.Context}]";

            if (log.Color.HasValue)
            {
                var color = ColorUtility.ToHtmlStringRGBA(log.Color.Value);
                contextTag = $"<color=#{color}>{contextTag}</color>";
            }

            return $"{contextTag} {log.Message}";
        }
    }
}
