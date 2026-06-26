using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Control;

public sealed class DefineSceneAndFrameLabelDataTag : Tag
{
    public SceneOffset[] Scenes { get; set; }

    public FrameLabel[] FrameLabels { get; set; }

    public DefineSceneAndFrameLabelDataTag(TagMetadata metadata, SceneOffset[] scenes, FrameLabel[] frameLabels) : base(metadata)
    {
        Scenes = scenes;
        FrameLabels = frameLabels;
    }

    public static DefineSceneAndFrameLabelDataTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var numScenes = reader.ReadEncodedU32();
        EnsureCount(numScenes, reader);
        var scenes = new SceneOffset[numScenes];

        for (var i = 0; i < numScenes; i++)
            scenes[i] = new SceneOffset(reader.ReadEncodedU32(), reader.ReadNullTerminatedString());

        var numFrameLabels = reader.ReadEncodedU32();
        EnsureCount(numFrameLabels, reader);
        var frameLabels = new FrameLabel[numFrameLabels];

        for (var i = 0; i < numFrameLabels; i++)
            frameLabels[i] = new FrameLabel(reader.ReadEncodedU32(), reader.ReadNullTerminatedString());

        return new DefineSceneAndFrameLabelDataTag(metadata, scenes, frameLabels);
    }

    private static void EnsureCount(uint count, MemoryReader reader)
    {
        if (count > (uint)reader.Remaining)
            throw new SwfFormatException($"Entry count {count} exceeds the {reader.Remaining} bytes remaining.");
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteEncodedU32((uint)Scenes.Length);

        foreach (var scene in Scenes)
        {
            writer.WriteEncodedU32(scene.Offset);
            writer.WriteNullTerminatedString(scene.Name);
        }

        writer.WriteEncodedU32((uint)FrameLabels.Length);

        foreach (var frameLabel in FrameLabels)
        {
            writer.WriteEncodedU32(frameLabel.FrameNum);
            writer.WriteNullTerminatedString(frameLabel.Label);
        }
    }
}
