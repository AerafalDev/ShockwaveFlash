// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Metadata;

public sealed record FileAttributesTag(TagMetadata Metadata, FileAttributesFlags Flags) : Tag(Metadata)
{
    public bool UseDirectBlit =>
        Flags.HasFlag(FileAttributesFlags.UseDirectBlit);

    public bool UseGpu =>
        Flags.HasFlag(FileAttributesFlags.UseGpu);

    public bool HasMetadata =>
        Flags.HasFlag(FileAttributesFlags.HasMetadata);

    public bool IsActionScript3 =>
        Flags.HasFlag(FileAttributesFlags.IsActionScript3);

    public bool UseNetworkSandbox =>
        Flags.HasFlag(FileAttributesFlags.UseNetworkSandbox);

    public static FileAttributesTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new FileAttributesTag(metadata, (FileAttributesFlags)reader.ReadUInt32());
    }
}
