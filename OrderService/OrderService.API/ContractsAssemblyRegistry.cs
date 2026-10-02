using System.Reflection;
using Contracts;
using MassTransit.Futures.Contracts;

namespace OrderService.API;

public class MessageTypeRegistry
{
    private Dictionary<string, Type> MsgTypeNames {get;  set; } = new Dictionary<string, Type>();
    public IReadOnlyDictionary<string, Type> GetMsgTypeNames() => MsgTypeNames.AsReadOnly();

    public void Build(Assembly contracts)
    {
        List<Exception> exceptions = new List<Exception>();
        Type[] types = contracts.GetTypes();
        
        IEnumerable<Type> messageTypes = types
            .Where(t => t.IsPublic)
            .Where(t => t.IsClass)
            .Where(t => !t.IsAbstract)
            .Where(t => !t.IsSubclassOf(typeof(Attribute)));
    
        foreach (Type type in messageTypes)
        {
            var msgAttribute = type.GetCustomAttribute<MessageTypeAttribute>();

            if (msgAttribute == null) {
                exceptions.Add(new InvalidOperationException($"Message type attribute not found in {type.Name}"));
                continue;
            }

            if (!MsgTypeNames.TryAdd(msgAttribute.MessageTypeName, type)) exceptions.Add(new InvalidOperationException($"There is already a message with type {msgAttribute.MessageTypeName} registered in {MsgTypeNames[msgAttribute.MessageTypeName]}"));            
        }

        if (exceptions.Count != 0) throw new AggregateException(exceptions);
    }
}