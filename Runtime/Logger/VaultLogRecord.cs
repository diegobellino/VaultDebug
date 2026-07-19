using Unity.Collections;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// The unmanaged representation written by OOP and Burst callers.
    /// </summary>
    internal struct VaultLogRecord
    {
        public LogLevel Level;
        public FixedString64Bytes Context;
        public FixedString512Bytes Message;
        public VaultLogProperties Properties;
        public byte HasColor;
        public byte ColorR;
        public byte ColorG;
        public byte ColorB;
        public byte ColorA;
    }
}
