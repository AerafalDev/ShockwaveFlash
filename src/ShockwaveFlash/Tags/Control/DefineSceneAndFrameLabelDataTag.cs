// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Control;

public sealed record DefineSceneAndFrameLabelDataTag(TagMetadata Metadata, SceneOffset[] Scenes, FrameLabel[] FrameLabels) : Tag(Metadata)
{
    public static DefineSceneAndFrameLabelDataTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var numScenes = reader.ReadEncodedU32();
        var scenes = new SceneOffset[numScenes];

        for (var i = 0; i < numScenes; i++)
            scenes[i] = new SceneOffset(reader.ReadEncodedU32(), reader.ReadNullTerminatedString());

        var numFrameLabels = reader.ReadEncodedU32();
        var frameLabels = new FrameLabel[numFrameLabels];

        for (var i = 0; i < numFrameLabels; i++)
            frameLabels[i] = new FrameLabel(reader.ReadEncodedU32(), reader.ReadNullTerminatedString());

        return new DefineSceneAndFrameLabelDataTag(metadata, scenes, frameLabels);
    }
}
