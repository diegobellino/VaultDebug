using System.Collections.Generic;
using Unity.Collections;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// The value kinds supported by the Burst-safe structured logging payload.
    /// </summary>
    public enum VaultLogPropertyType : byte
    {
        None,
        String,
        Int,
        Float,
        Bool
    }

    /// <summary>
    /// A single fixed-size, Burst-safe structured log property.
    /// </summary>
    public struct VaultLogProperty
    {
        public FixedString64Bytes Key;
        public VaultLogPropertyType Type;
        public FixedString128Bytes StringValue;
        public long IntValue;
        public double FloatValue;
        public byte BoolValue;

        public static VaultLogProperty String(FixedString64Bytes key, FixedString128Bytes value) =>
            new VaultLogProperty { Key = key, Type = VaultLogPropertyType.String, StringValue = value };

        public static VaultLogProperty Int(FixedString64Bytes key, long value) =>
            new VaultLogProperty { Key = key, Type = VaultLogPropertyType.Int, IntValue = value };

        public static VaultLogProperty Float(FixedString64Bytes key, double value) =>
            new VaultLogProperty { Key = key, Type = VaultLogPropertyType.Float, FloatValue = value };

        public static VaultLogProperty Bool(FixedString64Bytes key, bool value) =>
            new VaultLogProperty { Key = key, Type = VaultLogPropertyType.Bool, BoolValue = value ? (byte)1 : (byte)0 };

        internal object ToManagedValue()
        {
            return Type switch
            {
                VaultLogPropertyType.String => StringValue.ToString(),
                VaultLogPropertyType.Int => IntValue,
                VaultLogPropertyType.Float => FloatValue,
                VaultLogPropertyType.Bool => BoolValue != 0,
                _ => null
            };
        }
    }

    /// <summary>
    /// A bounded structured payload that can be created and passed through Burst jobs.
    /// </summary>
    public struct VaultLogProperties
    {
        public const int Capacity = 8;

        byte _count;
        VaultLogProperty _property0;
        VaultLogProperty _property1;
        VaultLogProperty _property2;
        VaultLogProperty _property3;
        VaultLogProperty _property4;
        VaultLogProperty _property5;
        VaultLogProperty _property6;
        VaultLogProperty _property7;

        /// <summary>Gets the number of properties currently stored.</summary>
        public int Count => _count;

        /// <summary>Adds a string property when capacity remains.</summary>
        public bool TryAdd(FixedString64Bytes key, FixedString128Bytes value) => TryAdd(VaultLogProperty.String(key, value));

        /// <summary>Adds an integer property when capacity remains.</summary>
        public bool TryAdd(FixedString64Bytes key, long value) => TryAdd(VaultLogProperty.Int(key, value));

        /// <summary>Adds a floating-point property when capacity remains.</summary>
        public bool TryAdd(FixedString64Bytes key, double value) => TryAdd(VaultLogProperty.Float(key, value));

        /// <summary>Adds a Boolean property when capacity remains.</summary>
        public bool TryAdd(FixedString64Bytes key, bool value) => TryAdd(VaultLogProperty.Bool(key, value));

        /// <summary>Adds a fixed-size property and returns false when the payload is full.</summary>
        public bool TryAdd(VaultLogProperty property)
        {
            switch (_count)
            {
                case 0: _property0 = property; _count++; return true;
                case 1: _property1 = property; _count++; return true;
                case 2: _property2 = property; _count++; return true;
                case 3: _property3 = property; _count++; return true;
                case 4: _property4 = property; _count++; return true;
                case 5: _property5 = property; _count++; return true;
                case 6: _property6 = property; _count++; return true;
                case 7: _property7 = property; _count++; return true;
                default: return false;
            }
        }

        internal Dictionary<string, object> ToManagedDictionary()
        {
            var properties = new Dictionary<string, object>(_count);
            for (var index = 0; index < _count; index++)
            {
                var property = Get(index);
                properties[property.Key.ToString()] = property.ToManagedValue();
            }

            return properties;
        }

        internal void Truncate(int maximumCount)
        {
            if (maximumCount < _count)
            {
                _count = (byte)maximumCount;
            }
        }

        VaultLogProperty Get(int index)
        {
            return index switch
            {
                0 => _property0,
                1 => _property1,
                2 => _property2,
                3 => _property3,
                4 => _property4,
                5 => _property5,
                6 => _property6,
                7 => _property7,
                _ => default
            };
        }
    }
}
