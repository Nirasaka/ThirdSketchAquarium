using TriLibCore.Collections;

namespace TriLibCore.Ply
{
    public class PlyElement
    {
        public readonly OrderedDictionary<long, PlyProperty> Properties = new OrderedDictionary<long, PlyProperty>();

        public int Count;

        public PlyValue[] Data;

        public void AddProperty(long name, PlyProperty property)
        {
            property.Offset = Properties.Count;
            Properties.Add(name, property);
        }

        public void AllocateData()
        {
            Data = new PlyValue[Count * Properties.Count];
        }

        public void SetData(int elementIndex, int propertyOffset, PlyValue value)
        {
            Data[elementIndex * Properties.Count + propertyOffset] = value;
        }

        public PlyProperty GetProperty(long propertyName)
        {
            Properties.TryGetValue(propertyName, out var property);
            return property;
        }

        public int GetListIndex(PlyListProperty property, int elementIndex)
        {
            return PlyValue.GetIntValue(Data[elementIndex * Properties.Count + property.Offset], PlyPropertyType.Int);
        }

        public int GetPropertyIntValue(PlyProperty property, int elementIndex)
        {
            return PlyValue.GetIntValue(Data[elementIndex * Properties.Count + property.Offset], property);
        }

        public float GetPropertyFloatValue(PlyProperty property, int elementIndex)
        {
            return PlyValue.GetFloatValue(Data[elementIndex * Properties.Count + property.Offset], property);
        }

        public int GetPropertyIntValue(long propertyName, int elementIndex)
        {
            if (Properties.TryGetValue(propertyName, out var property))
            {
                return PlyValue.GetIntValue(Data[elementIndex * Properties.Count + property.Offset], property);
            }
            return default;
        }

        public float GetPropertyFloatValue(long propertyName, int elementIndex)
        {
            if (Properties.TryGetValue(propertyName, out var property))
            {
                return PlyValue.GetFloatValue(Data[elementIndex * Properties.Count + property.Offset], property);
            }
            return default;
        }

        public PlyValue GetPropertyValue(long propertyName, int elementIndex)
        {
            if (Properties.TryGetValue(propertyName, out var property))
            {
                return Data[elementIndex * Properties.Count + property.Offset];
            }
            return default;
        }
    }
}