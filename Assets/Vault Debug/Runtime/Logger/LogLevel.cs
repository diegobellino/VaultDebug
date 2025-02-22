using System;

namespace VaultDebug.Runtime.Logger
{ 
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Info = 1, // 1
        Debug = 2, // 10
        Warn = 4, // 100
        Error = 8, // 1000
        Exception = 16 // 10000
    }
}
