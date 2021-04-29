using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;

namespace TNRD.Reflectives.Exporters
{
    public class FieldExporter
    {
        public static CodeTypeMemberCollection Generate(Type type)
        {
            FieldExporter fieldExporter = new FieldExporter(type);
            fieldExporter.Generate();
            return fieldExporter.members;
        }

        public static void Iterate(Type type, Action<FieldExporter, FieldInfo> action)
        {
            FieldExporter exporter = new FieldExporter(type);
            exporter.Iterate(action);
        }

        private readonly CodeTypeMemberCollection members = new CodeTypeMemberCollection();
        private readonly Type type;

        // For filtering out events
        private EventInfo[] eventInfos;

        private FieldExporter(Type type)
        {
            this.type = type;
        }

        private bool FilterEvents(FieldInfo fieldInfo)
        {
            if (eventInfos == null)
                eventInfos = EventExporter.GetEvents(type);

            return eventInfos.All(x => x.Name != fieldInfo.Name);
        }

        private void Iterate(Action<FieldExporter, FieldInfo> action)
        {
            FieldInfo[] fieldInfos = type.GetFields(Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .Where(x => !x.Name.Contains("<"))
                .Where(FilterEvents)
                .ToArray();

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                action.Invoke(this, fieldInfo);
            }
        }

        private void Generate()
        {
            void Action(FieldExporter exporter, FieldInfo fieldInfo)
            {
                GenerateField(fieldInfo);
                GenerateProperty(fieldInfo);
            }

            Iterate(Action);
        }

        private void GenerateField(FieldInfo fieldInfo)
        {
            CodeMemberField field = new CodeMemberField(typeof(ReflectiveField), $"field_{fieldInfo.Name}")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Final
            };

            members.Add(field);
        }

        private void GenerateProperty(FieldInfo fieldInfo)
        {
            CodeMemberProperty property = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                HasGet = true,
                HasSet = !fieldInfo.IsInitOnly,
                Name = fieldInfo.Name,
                Type = ExportUtils.GetCodeTypeReference(fieldInfo.FieldType)
            };

            AddPropertyGetter(property, fieldInfo);
            AddPropertySetter(property, fieldInfo);

            members.Add(property);
        }

        private void AddPropertyGetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            if (ExportUtils.IsPublic(fieldInfo.FieldType))
            {
                ExportFunctions.AddPublicGetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else if (fieldInfo.FieldType.IsEnum)
            {
                ExportFunctions.AddEnumGetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else if (ExportUtils.IsDictionary(fieldInfo.FieldType))
            {
                ExportFunctions.AddDictionaryGetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else if (ExportUtils.IsEnumerable(fieldInfo.FieldType) && !fieldInfo.FieldType.IsArray)
            {
                ExportFunctions.AddEnumerableGetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else if (fieldInfo.FieldType.IsArray)
            {
                ExportFunctions.AddArrayGetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else
            {
                ExportFunctions.AddDefaultGetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
        }

        private void AddPropertySetter(CodeMemberProperty property, FieldInfo fieldInfo)
        {
            if (!property.HasSet)
                return;

            if (ExportUtils.IsPublic(fieldInfo.FieldType))
            {
                ExportFunctions.AddPublicSetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else if (fieldInfo.FieldType.IsEnum)
            {
                ExportFunctions.AddEnumSetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else if (ExportUtils.IsDictionary(fieldInfo.FieldType))
            {
                ExportFunctions.AddDictionarySetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else if (ExportUtils.IsEnumerable(fieldInfo.FieldType) && !fieldInfo.FieldType.IsArray)
            {
                ExportFunctions.AddEnumerableSetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else if (fieldInfo.FieldType.IsArray)
            {
                ExportFunctions.AddArraySetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
            else
            {
                ExportFunctions.AddDefaultSetter(property, fieldInfo.FieldType, fieldInfo.Name, MemberTypes.Field);
            }
        }
    }
}
