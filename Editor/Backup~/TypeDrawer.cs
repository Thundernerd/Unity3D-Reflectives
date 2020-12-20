using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace TNRD.Reflectives
{
    [Serializable]
    public class TypeDrawer : ScriptableObject
    {
        [SerializeField]
        [TableList(IsReadOnly = true, AlwaysExpanded = true)]
        [TabGroup("Fields")]
        [HideLabel]
        private List<FieldInfoDrawer> fieldInfoDrawers = new List<FieldInfoDrawer>();

        [SerializeField]
        [TableList(IsReadOnly = true, AlwaysExpanded = true)]
        [TabGroup("Properties")]
        [HideLabel]
        private List<PropertyInfoDrawer> propertyInfoDrawers = new List<PropertyInfoDrawer>();

        [SerializeField]
        [TableList(IsReadOnly = true, AlwaysExpanded = true)]
        [TabGroup("Methods")]
        [HideLabel]
        private List<MethodInfoDrawer> methodInfoDrawers = new List<MethodInfoDrawer>();

        public List<FieldInfo> Fields =>
            fieldInfoDrawers
                .Where(x => x.Export)
                .Select(x => x.Field)
                .ToList();

        public List<PropertyInfo> Properties =>
            propertyInfoDrawers
                .Where(x => x.Export)
                .Select(x => x.Property)
                .ToList();

        public List<MethodInfo> Methods =>
            methodInfoDrawers
                .Where(x => x.Export)
                .Select(x => x.Method)
                .ToList();

        public void Load(Type type)
        {
            if (type == null)
                return;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            fieldInfoDrawers = type.GetFields(flags)
                .Where(x => IsValid(x, type))
                .OrderBy(x => x.DeclaringType.GetNiceName())
                .ThenBy(x => x.Name)
                .Select(x => new FieldInfoDrawer(x))
                .ToList();

            propertyInfoDrawers = type.GetProperties(flags)
                .Where(x => IsValid(x, type))
                .OrderBy(x => x.DeclaringType.GetNiceName())
                .ThenBy(x => x.Name)
                .Select(x => new PropertyInfoDrawer(x))
                .ToList();

            methodInfoDrawers = type.GetMethods(flags)
                .Where(x => IsValid(x, type))
                .OrderBy(x => x.DeclaringType.GetNiceName())
                .ThenBy(x => x.Name)
                .Where(x => !IsSpecialMethod(x))
                .Select(x => new MethodInfoDrawer(x))
                .ToList();
        }

        private bool IsValid(MemberInfo memberInfo, Type declaringType)
        {
            return memberInfo.DeclaringType == declaringType;
        }

        private bool IsSpecialMethod(MethodInfo methodInfo)
        {
            return methodInfo.Name.StartsWith("get_") ||
                   methodInfo.Name.StartsWith("set_") ||
                   methodInfo.Name.StartsWith("add_") ||
                   methodInfo.Name.StartsWith("remove_") ||
                   methodInfo.Name.StartsWith("<");
        }
    }
}