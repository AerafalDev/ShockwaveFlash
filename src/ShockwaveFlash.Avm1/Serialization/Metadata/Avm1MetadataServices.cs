using System.Collections.Generic;
using ShockwaveFlash.Avm1.Serialization.Converters;

namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public static class Avm1MetadataServices
{
    public static Avm1TypeInfo<T> CreateObjectInfo<T>(Avm1SerializerOptions options, Avm1ObjectInfoValues<T> values)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(values);

        var info = new Avm1TypeInfo<T>
        {
            Kind = Avm1TypeInfoKind.Object,
            Converter = new Avm1ObjectConverter<T>(),
            Options = options,
            ConstructorFactory = values.ConstructorFactory,
            ObjectFactory = values.ObjectCreator,
            ConstructorArguments = values.ConstructorArguments ?? [],
            BindingPath = values.BindingPath,
        };

        var initializer = values.PropertyMetadataInitializer;
        if (initializer is not null)
            info.SetPopulate(target =>
            {
                foreach (var property in initializer(options))
                    target.Properties.Add(property);
            });

        return info;
    }

    public static Avm1TypeInfo<TBase> CreatePolymorphicInfo<TBase>(Avm1SerializerOptions options, Avm1PolymorphicInfoValues<TBase> values)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(values);

        var info = new Avm1TypeInfo<TBase>
        {
            Kind = Avm1TypeInfoKind.None,
            Converter = new Avm1PolymorphicConverter<TBase>(),
            Options = options,
            BindingPath = values.BindingPath,
        };

        var polymorphism = new Avm1PolymorphismInfo { DiscriminatorName = values.DiscriminatorName };
        foreach (var (type, discriminator) in values.DerivedTypes)
            polymorphism.Add(type, discriminator);

        info.Polymorphism = polymorphism;
        return info;
    }

    public static Avm1PropertyInfo CreatePropertyInfo<T>(Avm1SerializerOptions options, Avm1PropertyInfoValues<T> values)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(values);

        var converter = values.ConverterType is not null
            ? options.GetConverterFromAttribute(values.ConverterType, values.MemberType)
            : options.GetConverter(values.MemberType);

        var name = !string.IsNullOrEmpty(values.Avm1PropertyName)
            ? values.Avm1PropertyName
            : options.PropertyNamingPolicy?.ConvertName(values.MemberName) ?? values.MemberName;

        return new Avm1PropertyInfo
        {
            Name = name,
            MemberName = values.MemberName,
            Get = values.Getter,
            Set = values.Setter,
            IsRequired = values.ThrowIfMissing,
            Converter = converter,
            Nullable = values.Nullable,
            ThrowIfMissing = values.ThrowIfMissing,
            IsValueScalar = values.IsValueScalar,
            UnderlyingType = Nullable.GetUnderlyingType(values.MemberType) ?? values.MemberType,
            IsConstructorParameter = values.IsConstructorParameter,
            Settable = values.Setter is not null,
            Order = values.Order,
            IsExtensionData = values.IsExtensionData,
        };
    }
}
