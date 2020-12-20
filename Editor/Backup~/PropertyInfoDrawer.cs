using System;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TNRD.Reflectives
{
    [Serializable]
    public class PropertyInfoDrawer
    {
        private readonly PropertyInfo property;

        [SerializeField]
        [TableColumnWidth(50, false)]
        private bool export;

        [SerializeField]
        [ReadOnly]
        private string type;

        [SerializeField]
        [ReadOnly]
        private string name;

        [SerializeField]
        [ReadOnly]
        [TableColumnWidth(75, false)]
        private bool readable;

        [SerializeField]
        [ReadOnly]
        [TableColumnWidth(75, false)]
        private bool writeable;

        [SerializeField]
        [ReadOnly]
        private string declaringType;

        public PropertyInfo Property => property;
        public bool Export => export;
        
        public PropertyInfoDrawer(PropertyInfo propertyInfo)
        {
            property = propertyInfo;

            type = property.PropertyType.GetNiceName();
            name = property.GetNiceName();
            readable = property.CanRead;
            writeable = property.CanWrite;
            declaringType = propertyInfo.DeclaringType.GetNiceName();
        }
    }
}