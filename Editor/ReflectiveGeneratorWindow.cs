using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using TNRD.Odin;
using UnityEditor;
using UnityEngine;

namespace TNRD.Reflectives
{
    internal class ReflectiveGeneratorWindow : OdinEditorWindow
    {
        public const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        [MenuItem("TNRD/Reflective Generator")]
        private static void Open()
        {
            ReflectiveGeneratorWindow wnd = GetWindow<ReflectiveGeneratorWindow>("Reflective Generator");
            wnd.Show();
        }

        private List<Type> types = new List<Type>();
        private ValueDropdownList<Type> typesList;

        [SerializeField]
        [FolderPath(AbsolutePath = true, RequireExistingPath = true)]
        private string outputDirectory;

        [SerializeField]
        private string @namespace;

        [SerializeField]
        private bool allowPublicTypes = true;

        [SerializeField]
        private bool inferTypes;

        [SerializeField]
        [ListDrawerSettings(Expanded = true, DraggableItems = false)]
        [ValueDropdown(nameof(GetTypes), IsUniqueList = true)]
        private List<Type> selectedTypes = new List<Type>();

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetTypes().Any())
                .OrderBy(x => x.GetName().FullName)
                .SelectMany(x => x.GetTypes().OrderBy(y => y.FullName).Where(y => !y.IsSpecialName && !y.FullName.Contains("<")))
                .ToList();

            typesList = new ValueDropdownList<Type>();

            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                typesList.Add(new ValueDropdownItem<Type>(type.FullName.Replace('.', '/'), type));
            }
        }

        private IEnumerable GetTypes()
        {
            return typesList;
        }

        [FlexibleSpace]
        [Button(ButtonSizes.Gigantic)]
        [DisableIf("@this.selectedTypes.Count == 0 || string.IsNullOrEmpty(this.outputDirectory) || !System.IO.Directory.Exists(this.outputDirectory)")]
        private void Export()
        {
            Debug.Log("[Reflective Generator] Starting export");
            IEnumerable<Type> inferredTypes = GetInferredTypes();

            List<Type> combined = new List<Type>(selectedTypes);
            combined.AddRange(inferredTypes);
            combined = combined
                .Distinct()
                .Where(IsPrivate)
                .ToList();

            foreach (Type type in combined)
            {
                Exporter exporter = new Exporter(@namespace, outputDirectory);
                exporter.Export(type);
                Debug.Log($"[Reflective Generator] Exported: {type.AssemblyQualifiedName}");
            }

            Debug.Log($"[Reflective Generator] Finished exporting {combined.Count} types");
            AssetDatabase.Refresh();
        }

        private IEnumerable<Type> GetInferredTypes()
        {
            if (!inferTypes)
                return new List<Type>();

            List<Type> types = new List<Type>();

            foreach (Type selectedType in selectedTypes)
            {
                List<Type> inferredTypes = TypeCrawler.GetInferredTypes(selectedType);
                types.AddRange(inferredTypes);
            }

            return types;
        }

        private static bool IsPublic(Type type)
        {
            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 0)
                return type.IsPublic;

            return type.IsPublic && genericArguments.All(IsPublic);
        }

        private bool IsPrivate(Type type)
        {
            return !IsPublic(type) || allowPublicTypes;
        }
    }
}