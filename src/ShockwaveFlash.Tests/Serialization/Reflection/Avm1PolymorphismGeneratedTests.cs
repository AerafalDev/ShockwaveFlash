using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

[Avm1Serializable(typeof(Shape))]
[Avm1Serializable(typeof(Animal))]
public partial class ShapeContext : Avm1SerializerContext;

public sealed class Avm1PolymorphismGeneratedTests
{
    [Fact]
    public void Generated_polymorphism_matches_reflection()
    {
        Shape shape = new Circle(2.0);

        var reflected = Avm1Serializer.Serialize<Shape>(shape).AsObject;
        var generated = Avm1Serializer.Serialize(shape, ShapeContext.Default.Shape).AsObject;

        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue();
        generated["$type"].AsString.ShouldBe("circle");

        Avm1Serializer.Deserialize(generated, ShapeContext.Default.Shape).ShouldBeOfType<Circle>().Radius.ShouldBe(2.0);
    }

    [Fact]
    public void Derived_types_are_registered_by_the_context()
    {
        ShapeContext.Default.GetTypeInfo(typeof(Shape)).ShouldNotBeNull();
        ShapeContext.Default.GetTypeInfo(typeof(Circle)).ShouldNotBeNull();
        ShapeContext.Default.GetTypeInfo(typeof(Square)).ShouldNotBeNull();
    }

    [Fact]
    public void Generated_polymorphism_discriminates_by_an_existing_field()
    {
        var original = new Avm1Object { Members = { ["t"] = new Avm1String("cat"), ["l"] = new Avm1Number(9) } };

        var animal = Avm1Serializer.Deserialize(original, ShapeContext.Default.Animal);
        animal.ShouldBeOfType<Cat>().Lives.ShouldBe(9);

        var written = Avm1Serializer.Serialize(animal, ShapeContext.Default.Animal).AsObject;
        Avm1Trees.DeepEquals(original, written).ShouldBeTrue();
    }
}
