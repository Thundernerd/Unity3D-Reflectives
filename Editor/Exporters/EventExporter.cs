using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace TNRD.Reflectives.Exporters
{
    internal class EventExporter : MemberExporter
    {
        public override void Export(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            ExportEvents(type, definitionWriter, constructionWriter, bodyWriter);
        }

        private void ExportEvents(Type type, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            EventInfo[] events = type.GetEvents(Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .ToArray();

            foreach (EventInfo eventInfo in events)
            {
                ExportEvent(eventInfo, definitionWriter, constructionWriter, bodyWriter);
            }
        }

        private void ExportEvent(EventInfo eventInfo, IndentedTextWriter definitionWriter, IndentedTextWriter constructionWriter, IndentedTextWriter bodyWriter)
        {
            string memberName = eventInfo.GetNiceName();
            string uppercasedMemberName = $"{char.ToUpper(memberName[0])}{memberName.Substring(1)}";

            definitionWriter.WriteLine($"private ReflectiveEvent event_{memberName};");
            constructionWriter.WriteLine($"event_{memberName} = CreateEvent(\"{memberName}\", {GetBindingFlags(eventInfo)});");

            bodyWriter.WriteLine("/// <summary>");
            bodyWriter.WriteLine($"/// Event type: {eventInfo.EventHandlerType.GetNiceFullName()}");
            bodyWriter.WriteLine("/// </summary>");
            bodyWriter.WriteLine("/// <returns>Delegate to be used for unsubscribing</returns>");
            bodyWriter.WriteLine($"public Delegate SubscribeTo{uppercasedMemberName}(Delegate @delegate)");
            bodyWriter.WriteLine("{");
            bodyWriter.Indent++;
            bodyWriter.WriteLine($"return event_{memberName}.Subscribe(@delegate);");
            bodyWriter.Indent--;
            bodyWriter.WriteLine("}");

            bodyWriter.WriteLine("/// <summary>");
            bodyWriter.WriteLine($"/// Event type: {eventInfo.EventHandlerType.GetNiceFullName()}");
            bodyWriter.WriteLine("/// </summary>");
            bodyWriter.WriteLine($"public void UnsubscribeFrom{uppercasedMemberName}(Delegate @delegate)");
            bodyWriter.WriteLine("{");
            bodyWriter.Indent++;
            bodyWriter.WriteLine($"event_{memberName}.Unsubscribe(@delegate);");
            bodyWriter.Indent--;
            bodyWriter.WriteLine("}");
        }
    }
}