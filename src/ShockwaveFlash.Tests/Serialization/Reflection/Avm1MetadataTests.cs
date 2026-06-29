using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed class Avm1MetadataTests
{
    [Fact]
    public void Explicit_default_resolver_round_trips()
    {
        var options = new Avm1SerializerOptions { TypeInfoResolver = new DefaultAvm1TypeInfoResolver() };

        var obj = Avm1Serializer.Serialize(new Coord(3, 4), options).AsObject;
        obj["X"].AsNumber.ShouldBe(3d);

        Avm1Serializer.Deserialize<Coord>(obj, options).ShouldBe(new Coord(3, 4));
    }

    [Fact]
    public void Default_resolver_describes_the_type_shape()
    {
        var resolver = new DefaultAvm1TypeInfoResolver();

        var info = resolver.GetTypeInfo(typeof(Coord), new Avm1SerializerOptions());
        info.ShouldNotBeNull();
        info.Type.ShouldBe(typeof(Coord));
        info.Kind.ShouldBe(Avm1TypeInfoKind.Object);

        var scalar = resolver.GetTypeInfo(typeof(int), new Avm1SerializerOptions());
        scalar.ShouldNotBeNull();
        scalar.Kind.ShouldBe(Avm1TypeInfoKind.None);
    }

    [Fact]
    public void Combine_falls_through_to_the_next_resolver()
    {
        var options = new Avm1SerializerOptions
        {
            TypeInfoResolver = Avm1TypeInfoResolver.Combine(new EmptyResolver(), new DefaultAvm1TypeInfoResolver()),
        };

        Avm1Serializer.Serialize(new Coord(1, 2), options).AsObject["Y"].AsNumber.ShouldBe(2d);
    }

    [Fact]
    public void Resolver_without_metadata_throws()
    {
        var options = new Avm1SerializerOptions { TypeInfoResolver = new EmptyResolver() };

        Should.Throw<Avm1SerializationException>(() => Avm1Serializer.Serialize(new Coord(1, 2), options));
    }

    [Fact]
    public void Context_owns_its_options_and_resolves_known_types()
    {
        var context = new CoordContext(null);
        var options = new Avm1SerializerOptions { TypeInfoResolver = context };

        context.Options.ShouldNotBeNull();
        Avm1Serializer.Serialize(new Coord(5, 6), options).AsObject["X"].AsNumber.ShouldBe(5d);
        Should.Throw<Avm1SerializationException>(() => Avm1Serializer.Serialize(new Settings(), options));
    }

    [Fact]
    public void Context_combined_with_default_serves_everything()
    {
        var options = new Avm1SerializerOptions
        {
            TypeInfoResolver = Avm1TypeInfoResolver.Combine(new CoordContext(null), new DefaultAvm1TypeInfoResolver()),
        };

        Avm1Serializer.Serialize(new Coord(7, 8), options).AsObject["X"].AsNumber.ShouldBe(7d);
        Avm1Serializer.Serialize(new Settings { Theme = "dark" }, options).AsObject["Theme"].AsString.ShouldBe("dark");
    }

    [Fact]
    public void Self_referential_type_round_trips()
    {
        var value = new TreeNode("a", new TreeNode("b", new TreeNode("c", null)));

        var obj = Avm1Serializer.Serialize(value).AsObject;
        obj["Label"].AsString.ShouldBe("a");
        obj["Next"].AsObject["Next"].AsObject["Label"].AsString.ShouldBe("c");

        var back = Avm1Serializer.Deserialize<TreeNode>(obj);
        back.Next!.Next!.Label.ShouldBe("c");
        back.Next.Next.Next.ShouldBeNull();
    }

    private sealed class EmptyResolver : IAvm1TypeInfoResolver
    {
        public Avm1TypeInfo? GetTypeInfo(Type type, Avm1SerializerOptions options)
        {
            return null;
        }
    }

    private sealed class CoordContext : Avm1SerializerContext
    {
        private readonly DefaultAvm1TypeInfoResolver _fallback = new();

        public CoordContext(Avm1SerializerOptions? options)
            : base(options)
        {
        }

        public override Avm1TypeInfo? GetTypeInfo(Type type)
        {
            return type == typeof(Coord) ? _fallback.GetTypeInfo(type, Options) : null;
        }
    }
}
