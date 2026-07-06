using System.Collections.Generic;

namespace TriLibCore.Ply
{
    public struct PlyValue
    {
        private uint _intValue;
        public static PlyValue Unknown;

        public static implicit operator PlyValue(sbyte other)
        {
            return new PlyValue { _intValue = (uint)(other + sbyte.MaxValue) };
        }

        public static implicit operator PlyValue(byte other)
        {
            return new PlyValue { _intValue = other };
        }

        public static implicit operator PlyValue(ushort other)
        {
            return new PlyValue { _intValue = other };
        }

        public static implicit operator PlyValue(short other)
        {
            return new PlyValue { _intValue = (uint)(other + short.MaxValue) };
        }

        public static implicit operator PlyValue(uint other)
        {
            return new PlyValue { _intValue = other };
        }

        public static implicit operator PlyValue(int other)
        {
            return new PlyValue { _intValue = (uint)(other + int.MaxValue) };
        }

        public static implicit operator PlyValue(float other)
        {
            unsafe
            {
                return new PlyValue { _intValue = (uint)*(int*)&other };
            }
        }

        public static implicit operator PlyValue(double other)
        {
            unsafe
            {
                var value = (float)other;
                return new PlyValue { _intValue = (uint)*(int*)&value };
            }
        }

        public static int GetIntValue(PlyValue value, PlyProperty property)
        {
            return GetIntValue(value, property.Type);
        }

        public static int GetIntValue(PlyValue value, PlyPropertyType propertyType)
        {
            switch (propertyType)
            {
                case PlyPropertyType.UChar:
                case PlyPropertyType.UInt:
                case PlyPropertyType.UShort:
                    return (int)value._intValue;
                case PlyPropertyType.Char:
                    return (int)(value._intValue - sbyte.MaxValue);
                case PlyPropertyType.Short:
                    return (int)(value._intValue - short.MaxValue);
                case PlyPropertyType.Int:
                    return (int)(value._intValue - int.MaxValue);
                case PlyPropertyType.Double:
                case PlyPropertyType.Float:
                    unsafe
                    {
                        return (int)*(float*)&value._intValue;
                    }
                case PlyPropertyType.List:
                case PlyPropertyType.Custom:
                    return 0;
            }
            return 0;
        }

        public static float GetFloatValue(PlyValue value, PlyProperty property)
        {
            return GetFloatValue(value, property.Type);
        }

        public static float GetFloatValue(PlyValue value, PlyPropertyType propertyType)
        {
            switch (propertyType)
            {
                case PlyPropertyType.UChar:
                case PlyPropertyType.UInt:
                case PlyPropertyType.UShort:
                    return (float)value._intValue;
                case PlyPropertyType.Char:
                    return (float)(value._intValue - sbyte.MaxValue);
                case PlyPropertyType.Short:
                    return (float)(value._intValue - short.MaxValue);
                case PlyPropertyType.Int:
                    return (float)(value._intValue - int.MaxValue);
                case PlyPropertyType.Double:
                case PlyPropertyType.Float:
                    unsafe
                    {
                        return *(float*)&value._intValue;
                    }
                case PlyPropertyType.List:
                case PlyPropertyType.Custom:
                    return 0f;
            }
            return 0f;
        }

        public static List<PlyValue> GetListValue(int index, List<List<PlyValue>> lists)
        {
            return index < lists.Count ? lists[index] : null;
        }
    }
}