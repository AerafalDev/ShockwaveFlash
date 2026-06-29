using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed class Avm1ContractTests
{
    [Fact]
    public void Reflection_is_enabled_by_default()
    {
        Avm1Serializer.IsReflectionEnabledByDefault.ShouldBeTrue();
    }

    [Fact]
    public void A_modifier_customizes_the_contract()
    {
        var options = new Avm1SerializerOptions();
        options.Modifiers.Add(info =>
        {
            foreach (var property in info.Properties)
                if (property.Name == "X")
                    property.Name = "x_renamed";
        });

        var obj = Avm1Serializer.Serialize(new Coord(1, 2), options).AsObject;

        obj.Members.ShouldContainKey("x_renamed");
        obj.Members.ShouldNotContainKey("X");
        obj["x_renamed"].AsNumber.ShouldBe(1d);
    }
}
