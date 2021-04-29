using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;

namespace TNRD.Reflectives.Exporters
{
    public class EventExporter
    {
        public static CodeTypeMemberCollection Generate(Type type)
        {
            EventExporter exporter = new EventExporter(type);
            exporter.Generate();
            return exporter.members;
        }

        private readonly CodeTypeMemberCollection members = new CodeTypeMemberCollection();
        private readonly Type type;

        private EventExporter(Type type)
        {
            this.type = type;
        }

        public static EventInfo[] GetEvents(Type type)
        {
            return type.GetEvents(Reflectives.Exporter.FLAGS)
                .Where(x => x.DeclaringType == type)
                .ToArray();
        }

        private void Iterate(Action<EventExporter, EventInfo> action)
        {
            EventInfo[] events = GetEvents(type);

            foreach (EventInfo eventInfo in events)
            {
                action.Invoke(this, eventInfo);
            }
        }

        private void Generate()
        {
            void Action(EventExporter exporter, EventInfo eventInfo)
            {
                GenerateField(eventInfo);
                GenerateSubscribeMethod(eventInfo);
                GenerateUnsubscribeMethod(eventInfo);
            }

            Iterate(Action);
        }

        private void GenerateField(EventInfo eventInfo)
        {
            CodeMemberField field = new CodeMemberField(typeof(ReflectiveEvent), $"event_{eventInfo.Name}")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Final
            };

            members.Add(field);
        }

        private void GenerateSubscribeMethod(EventInfo eventInfo)
        {
            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = $"SubscribeTo{eventInfo.Name}",
                ReturnType = new CodeTypeReference(typeof(Delegate))
            };

            method.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    typeof(Delegate),
                    "@delegate"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(),
                                $"event_{eventInfo.Name}"),
                            nameof(ReflectiveEvent.Subscribe),
                            new CodeArgumentReferenceExpression("@delegate")
                        )));

            members.Add(method);
        }

        private void GenerateUnsubscribeMethod(EventInfo eventInfo)
        {
            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = $"UnsubscribeFrom{eventInfo.Name}"
            };

            method.Parameters.Add(
                new CodeParameterDeclarationExpression(
                    typeof(Delegate),
                    "@delegate"));

            method.Statements.Add(
                new CodeMethodInvokeExpression(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            $"event_{eventInfo.Name}"),
                        nameof(ReflectiveEvent.Unsubscribe),
                        new CodeArgumentReferenceExpression("@delegate")
                    ));

            members.Add(method);
        }
    }
}
