using System;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TNRD.Reflectives
{
    [Serializable]
    public class FieldInfoDrawer
    {
        private readonly FieldInfo field;

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
        private bool @static;

        [SerializeField]
        [ReadOnly]
        [TableColumnWidth(75, false)]
        private bool readOnly;

        [SerializeField]
        [ReadOnly]
        private string declaringType;

        public FieldInfo Field => field;
        public bool Export => export;

        public FieldInfoDrawer(FieldInfo fieldInfo)
        {
            field = fieldInfo;

            type = field.FieldType.GetNiceName();
            name = field.GetNiceName();
            @static = field.IsStatic;
            readOnly = field.IsInitOnly;
            declaringType = fieldInfo.DeclaringType.GetNiceName();
        }
    }
}