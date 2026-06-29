using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

[Avm1Polymorphic]
[Avm1DerivedType(typeof(Circle), "circle")]
[Avm1DerivedType(typeof(Square), "square")]
public abstract record Shape;

public sealed record Circle([property: Avm1Property("r")] double Radius) : Shape;

public sealed record Square([property: Avm1Property("s")] double Side) : Shape;

public sealed record Drawing(Shape Item);

[Avm1Polymorphic(TypeDiscriminatorPropertyName = "t")]
[Avm1DerivedType(typeof(Dog), "dog")]
[Avm1DerivedType(typeof(Cat), "cat")]
public abstract record Animal;

public sealed record Dog([property: Avm1Property("n")] string Name) : Animal;

public sealed record Cat([property: Avm1Property("l")] int Lives) : Animal;

public sealed class Avm1PolymorphismReflectionTests
{
    [Fact]
    public void Discriminates_by_an_existing_avm1_field_and_round_trips_faithfully()
    {
        var original = new Avm1Object { Members = { ["t"] = new Avm1String("dog"), ["n"] = new Avm1String("Rex") } };

        var animal = Avm1Serializer.Deserialize<Animal>(original);
        animal.ShouldBeOfType<Dog>().Name.ShouldBe("Rex");

        var written = Avm1Serializer.Serialize<Animal>(animal).AsObject;
        written["t"].AsString.ShouldBe("dog");
        written["n"].AsString.ShouldBe("Rex");

        Avm1Trees.DeepEquals(original, written).ShouldBeTrue();
    }

    [Fact]
    public void Writes_the_discriminator_and_the_derived_members()
    {
        var obj = Avm1Serializer.Serialize<Shape>(new Circle(2.0)).AsObject;

        obj["$type"].AsString.ShouldBe("circle");
        obj["r"].AsNumber.ShouldBe(2.0);
    }

    [Fact]
    public void Reads_the_derived_type_from_the_discriminator()
    {
        var circle = Avm1Serializer.Serialize<Shape>(new Circle(2.0));
        Avm1Serializer.Deserialize<Shape>(circle).ShouldBeOfType<Circle>().Radius.ShouldBe(2.0);

        var square = Avm1Serializer.Serialize<Shape>(new Square(3.0));
        Avm1Serializer.Deserialize<Shape>(square).ShouldBeOfType<Square>().Side.ShouldBe(3.0);
    }

    [Fact]
    public void Works_as_a_member()
    {
        var obj = Avm1Serializer.Serialize(new Drawing(new Square(4.0))).AsObject;

        obj["Item"].AsObject["$type"].AsString.ShouldBe("square");
        obj["Item"].AsObject["s"].AsNumber.ShouldBe(4.0);

        Avm1Serializer.Deserialize<Drawing>(obj).Item.ShouldBeOfType<Square>().Side.ShouldBe(4.0);
    }

    [Fact]
    public void Unknown_discriminator_throws()
    {
        var obj = new Avm1Object { Members = { ["$type"] = new Avm1String("triangle") } };

        Should.Throw<ShockwaveFlash.Avm1.Exceptions.Avm1SerializationException>(() => Avm1Serializer.Deserialize<Shape>(obj));
    }
}
