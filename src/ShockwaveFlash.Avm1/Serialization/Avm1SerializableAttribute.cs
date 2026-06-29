namespace ShockwaveFlash.Avm1.Serialization;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class Avm1SerializableAttribute : Attribute
{
    public Avm1SerializableAttribute(Type type, string? bindingPath = null)
    {
        Type = type;
        BindingPath = bindingPath;
    }

    public Type Type { get; }

    public string? BindingPath { get; }

    public string[]? Segments { get; set; }

    public string? TypeInfoPropertyName { get; set; }
}
