using System;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TNRD.Reflectives
{
    [Serializable]
    public class MethodInfoDrawer
    {
        private MethodInfo method;

        [SerializeField]
        [TableColumnWidth(50, false)]
        private bool export;

        [SerializeField]
        [ReadOnly]
        private string returnType;

        [SerializeField]
        [ReadOnly]
        private string name;

        [SerializeField]
        [ReadOnly]
        [ListDrawerSettings(Expanded = true, IsReadOnly = true, HideAddButton = true, HideRemoveButton = true)]
        private string[] parameters;

        [SerializeField]
        [ReadOnly]
        private string declaringType;

        public MethodInfo Method => method;
        public bool Export => export;

        public MethodInfoDrawer(MethodInfo methodInfo)
        {
            method = methodInfo;

            returnType = methodInfo.ReturnType.GetNiceName();
            name = methodInfo.GetNiceName();
            parameters = methodInfo.GetParamsNames().Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            declaringType = methodInfo.DeclaringType.GetNiceName();
        }
    }
}