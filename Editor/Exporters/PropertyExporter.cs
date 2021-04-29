using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;

namespace TNRD.Reflectives.Exporters
{
    public class PropertyExporter
    {
        public static CodeTypeMemberCollection Generate(Type type)
        {
            PropertyExporter exporter = new PropertyExporter(type);
            exporter.Generate();
            return exporter.members;
        }

        public static void Iterate(Type type, Action<PropertyExporter, PropertyInfo> action)
        {
            PropertyExporter exporter = new PropertyExporter(type);
            exporter.Iterate(action);
        }

        private readonly CodeTypeMemberCollection members = new CodeTypeMemberCollection();
        private readonly Type type;

        private PropertyExporter(Type type)
        {
            this.type = type;
        }

        private void Iterate(Action<PropertyExporter, PropertyInfo> action)
        {
            PropertyInfo[] propertyInfos = type.GetProperties(Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .Where(x => !x.Name.Contains("<"))
                .ToArray();

            foreach (PropertyInfo info in propertyInfos)
            {
                action.Invoke(this, info);
            }
        }

        private void Generate()
        {
            void Action(PropertyExporter exporter, PropertyInfo propertyInfo)
            {
                GenerateField(propertyInfo);
                GenerateProperty(propertyInfo);
            }

            Iterate(Action);
        }

        private void GenerateField(PropertyInfo propertyInfo)
        {
            CodeMemberField field = new CodeMemberField(typeof(ReflectiveProperty), $"property_{propertyInfo.Name}")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Final
            };

            members.Add(field);
        }

        private void GenerateProperty(PropertyInfo propertyInfo)
        {
            CodeMemberProperty property = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                HasGet = propertyInfo.CanRead,
                HasSet = propertyInfo.CanWrite,
                Name = propertyInfo.Name,
                Type = ExportUtils.GetCodeTypeReference(propertyInfo.PropertyType),
            };

            AddPropertyGetter(property, propertyInfo);
            AddPropertySetter(property, propertyInfo);

            members.Add(property);
        }

        private void AddPropertyGetter(CodeMemberProperty property, PropertyInfo propertyInfo)
        {
            if (ExportUtils.IsPublic(propertyInfo.PropertyType))
            {
                ExportFunctions.AddPublicGetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                ExportFunctions.AddEnumGetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else if (ExportUtils.IsDictionary(propertyInfo.PropertyType))
            {
                ExportFunctions.AddDictionaryGetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else if (ExportUtils.IsEnumerable(propertyInfo.PropertyType) && !propertyInfo.PropertyType.IsArray)
            {
                ExportFunctions.AddEnumerableGetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else if (propertyInfo.PropertyType.IsArray)
            {
                ExportFunctions.AddArrayGetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else
            {
                ExportFunctions.AddDefaultGetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
        }

        private void AddPropertySetter(CodeMemberProperty property, PropertyInfo propertyInfo)
        {
            if (!property.HasSet)
                return;

            if (ExportUtils.IsPublic(propertyInfo.PropertyType))
            {
                ExportFunctions.AddPublicSetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                ExportFunctions.AddEnumSetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else if (ExportUtils.IsDictionary(propertyInfo.PropertyType))
            {
                ExportFunctions.AddDictionarySetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else if (ExportUtils.IsEnumerable(propertyInfo.PropertyType) && !propertyInfo.PropertyType.IsArray)
            {
                ExportFunctions.AddEnumerableSetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else if (propertyInfo.PropertyType.IsArray)
            {
                ExportFunctions.AddArraySetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
            else
            {
                ExportFunctions.AddDefaultSetter(property, propertyInfo.PropertyType, propertyInfo.Name, MemberTypes.Property);
            }
        }
    }
}
