using ShockwaveFlash.Avm1.Serialization;
using ShockwaveFlash.Avm1.Serialization.Metadata;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests.Serialization.Reflection;

public sealed class Avm1GeneratedContextShapeTests
{
    [Fact]
    public void Generated_shape_matches_reflection_for_constructor_type()
    {
        var value = new Coord(3, 4);

        var reflected = Avm1Serializer.Serialize(value);
        var generated = Avm1Serializer.Serialize(value, ProofContext.Default.Coord);

        Avm1Trees.DeepEquals(reflected, generated).ShouldBeTrue();
        Avm1Serializer.Deserialize(generated, ProofContext.Default.Coord).ShouldBe(value);
    }

    [Fact]
    public void Generated_shape_handles_init_only_object_initializer_type()
    {
        var value = new Settings { Theme = "dark", Mute = true };

        var generated = Avm1Serializer.Serialize(value, ProofContext.Default.Settings).AsObject;
        generated["Theme"].AsString.ShouldBe("dark");
        generated["Mute"].AsBoolean.ShouldBeTrue();

        var back = Avm1Serializer.Deserialize(generated, ProofContext.Default.Settings);
        back.Theme.ShouldBe("dark");
        back.Mute.ShouldBeTrue();
    }

    [Fact]
    public void Static_default_singleton_resolves_only_registered_types()
    {
        ProofContext.Default.ShouldBeSameAs(ProofContext.Default);
        ProofContext.Default.GetTypeInfo(typeof(Coord)).ShouldNotBeNull();
        ProofContext.Default.GetTypeInfo(typeof(int)).ShouldBeNull();
    }

    [Fact]
    public void Path_binding_reads_and_writes_a_named_global()
    {
        var globals = new Avm1Object();
        ProofContext.Default.Write(globals, new Coord(1, 2));

        globals.Members["pos"].AsObject["X"].AsNumber.ShouldBe(1d);
        ProofContext.Default.Read<Coord>(globals).ShouldBe(new Coord(1, 2));
        ProofContext.Default.Read<Coord>(new Avm1Object()).ShouldBeNull();
    }

    [Fact]
    public void Path_binding_creates_intermediate_objects()
    {
        var globals = new Avm1Object();
        DeepContext.Default.Write(globals, new Coord(7, 8));

        globals.Members["a"].AsObject["b"].AsObject["X"].AsNumber.ShouldBe(7d);
        DeepContext.Default.Read<Coord>(globals).ShouldBe(new Coord(7, 8));
    }

    private sealed class ProofContext : Avm1SerializerContext
    {
        public static ProofContext Default { get; } = new();

        public ProofContext()
            : base(null)
        {
        }

        private Avm1TypeInfo<Coord>? _coord;
        public Avm1TypeInfo<Coord> Coord => _coord ??= CreateCoord(Options);

        private Avm1TypeInfo<Settings>? _settings;
        public Avm1TypeInfo<Settings> Settings => _settings ??= CreateSettings(Options);

        public override Avm1TypeInfo? GetTypeInfo(Type type)
        {
            if (type == typeof(Coord))
                return Coord;
            if (type == typeof(Settings))
                return Settings;

            return null;
        }

        private static Avm1TypeInfo<Coord> CreateCoord(Avm1SerializerOptions options)
        {
            return Avm1MetadataServices.CreateObjectInfo(options, new Avm1ObjectInfoValues<Coord>
            {
                ConstructorFactory = static args => new Coord((int)args[0]!, (int)args[1]!),
                ConstructorArguments = ["X", "Y"],
                BindingPath = ["pos"],
                PropertyMetadataInitializer = static o =>
                [
                    Avm1MetadataServices.CreatePropertyInfo(o, new Avm1PropertyInfoValues<Coord>
                    {
                        MemberName = "X",
                        Avm1PropertyName = "X",
                        MemberType = typeof(int),
                        Getter = static obj => ((Coord)obj).X,
                        IsValueScalar = true,
                        IsConstructorParameter = true,
                    }),
                    Avm1MetadataServices.CreatePropertyInfo(o, new Avm1PropertyInfoValues<Coord>
                    {
                        MemberName = "Y",
                        Avm1PropertyName = "Y",
                        MemberType = typeof(int),
                        Getter = static obj => ((Coord)obj).Y,
                        IsValueScalar = true,
                        IsConstructorParameter = true,
                    }),
                ],
            });
        }

        private static Avm1TypeInfo<Settings> CreateSettings(Avm1SerializerOptions options)
        {
            return Avm1MetadataServices.CreateObjectInfo(options, new Avm1ObjectInfoValues<Settings>
            {
                ConstructorFactory = static args => new Settings { Theme = (string?)args[0], Mute = (bool)args[1]! },
                ConstructorArguments = ["Theme", "Mute"],
                BindingPath = ["cfg"],
                PropertyMetadataInitializer = static o =>
                [
                    Avm1MetadataServices.CreatePropertyInfo(o, new Avm1PropertyInfoValues<Settings>
                    {
                        MemberName = "Theme",
                        Avm1PropertyName = "Theme",
                        MemberType = typeof(string),
                        Getter = static obj => ((Settings)obj).Theme,
                        Nullable = true,
                        IsConstructorParameter = true,
                    }),
                    Avm1MetadataServices.CreatePropertyInfo(o, new Avm1PropertyInfoValues<Settings>
                    {
                        MemberName = "Mute",
                        Avm1PropertyName = "Mute",
                        MemberType = typeof(bool),
                        Getter = static obj => ((Settings)obj).Mute,
                        IsValueScalar = true,
                        IsConstructorParameter = true,
                    }),
                ],
            });
        }
    }

    private sealed class DeepContext : Avm1SerializerContext
    {
        public static DeepContext Default { get; } = new();

        public DeepContext()
            : base(null)
        {
        }

        private Avm1TypeInfo<Coord>? _coord;
        public Avm1TypeInfo<Coord> Coord => _coord ??= Create(Options);

        public override Avm1TypeInfo? GetTypeInfo(Type type)
        {
            return type == typeof(Coord) ? Coord : null;
        }

        private static Avm1TypeInfo<Coord> Create(Avm1SerializerOptions options)
        {
            return Avm1MetadataServices.CreateObjectInfo(options, new Avm1ObjectInfoValues<Coord>
            {
                ConstructorFactory = static args => new Coord((int)args[0]!, (int)args[1]!),
                ConstructorArguments = ["X", "Y"],
                BindingPath = ["a", "b"],
                PropertyMetadataInitializer = static o =>
                [
                    Avm1MetadataServices.CreatePropertyInfo(o, new Avm1PropertyInfoValues<Coord>
                    {
                        MemberName = "X",
                        Avm1PropertyName = "X",
                        MemberType = typeof(int),
                        Getter = static obj => ((Coord)obj).X,
                        IsValueScalar = true,
                        IsConstructorParameter = true,
                    }),
                    Avm1MetadataServices.CreatePropertyInfo(o, new Avm1PropertyInfoValues<Coord>
                    {
                        MemberName = "Y",
                        Avm1PropertyName = "Y",
                        MemberType = typeof(int),
                        Getter = static obj => ((Coord)obj).Y,
                        IsValueScalar = true,
                        IsConstructorParameter = true,
                    }),
                ],
            });
        }
    }
}
