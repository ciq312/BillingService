using System.Reflection.Metadata;

namespace Contracts;

[AttributeUsage(AttributeTargets.Class)]
public class MessageTypeAttribute : Attribute
{
    public string MessageTypeName {get; set;}
    public MessageTypeAttribute(string typeName)
    {
        ValidateTypeName(typeName);
        MessageTypeName = typeName.ToLower();
    }

    private void ValidateTypeName(string typeName)
    {
        if (typeName.Trim() == "") throw new ArgumentException("message type name can't be empty");
    }
}