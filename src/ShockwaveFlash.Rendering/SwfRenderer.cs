using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Rendering.Diagnostics;
using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Model.Sprites;
using ShockwaveFlash.Rendering.Processing;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.Bitmap;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Tags.Sprite;

namespace ShockwaveFlash.Rendering;

public sealed class SwfRenderer : IImageResolver, ICharacterResolver
{
    private readonly ShockwaveFlashFile _file;

    private readonly RenderOptions _options;

    private readonly ShapeProcessor _shapeProcessor;

    private readonly TimelineProcessor _timelineProcessor;

    private readonly Dictionary<int, IImage?> _imageCache = new();

    private Dictionary<int, Tag>? _imageTags;

    private Dictionary<int, ShapeDefinition>? _shapes;

    private Dictionary<int, SpriteDefinition>? _sprites;

    public SwfRenderer(ShockwaveFlashFile file, RenderOptions? options = null)
    {
        _file = file;
        _options = options ?? new RenderOptions();
        _shapeProcessor = new ShapeProcessor(this, _options);
        _timelineProcessor = new TimelineProcessor(this, _options);
    }

    public IDrawable Character(int characterId)
    {
        if (Shape(characterId) is { } shape)
            return shape;

        if (Sprite(characterId) is { } sprite)
            return sprite;

        if (ResolveImage(characterId) is { } image)
            return new ImageDrawable(image);

        return MissingCharacter.Instance;
    }

    public ShapeDefinition? Shape(int characterId)
    {
        _shapes ??= IndexShapes();
        return _shapes.GetValueOrDefault(characterId);
    }

    public SpriteDefinition? Sprite(int characterId)
    {
        _sprites ??= IndexSprites();
        return _sprites.GetValueOrDefault(characterId);
    }

    public IImage? ResolveImage(int characterId)
    {
        if (_imageCache.TryGetValue(characterId, out var cached))
            return cached;

        _imageTags ??= IndexImageTags();

        var image = _imageTags.TryGetValue(characterId, out var tag) ? Decode(tag) : null;
        _imageCache[characterId] = image;
        return image;
    }

    private Dictionary<int, ShapeDefinition> IndexShapes()
    {
        var map = new Dictionary<int, ShapeDefinition>();

        foreach (var tag in _file.Tags)
            if (tag is DefineShapeTag shape)
                map[shape.ShapeId] = new ShapeDefinition(_shapeProcessor, shape);

        return map;
    }

    private Dictionary<int, SpriteDefinition> IndexSprites()
    {
        var map = new Dictionary<int, SpriteDefinition>();

        foreach (var tag in _file.Tags)
            if (tag is DefineSpriteTag sprite)
                map[sprite.Id] = new SpriteDefinition(_timelineProcessor, sprite);

        return map;
    }

    private Dictionary<int, Tag> IndexImageTags()
    {
        var map = new Dictionary<int, Tag>();

        foreach (var tag in _file.Tags)
        {
            switch (tag)
            {
                case DefineBitsLosslessTag lossless:
                    map[lossless.Id] = lossless;
                    break;

                case DefineBitsLossless2Tag lossless2:
                    map[lossless2.Id] = lossless2;
                    break;

                case DefineBitsJpeg2Tag jpeg2:
                    map[jpeg2.Id] = jpeg2;
                    break;

                case DefineBitsJpeg3Tag jpeg3:
                    map[jpeg3.Id] = jpeg3;
                    break;

                case DefineBitsJpeg4Tag jpeg4:
                    map[jpeg4.Id] = jpeg4;
                    break;

                default:
                    break;
            }
        }

        return map;
    }

    private IImage? Decode(Tag tag)
    {
        try
        {
            return tag switch
            {
                DefineBitsLosslessTag lossless => BitmapDecoder.Decode(lossless),
                DefineBitsLossless2Tag lossless2 => BitmapDecoder.Decode(lossless2),
                DefineBitsJpeg2Tag jpeg2 => BitmapDecoder.Decode(jpeg2),
                DefineBitsJpeg3Tag jpeg3 => BitmapDecoder.Decode(jpeg3),
                DefineBitsJpeg4Tag jpeg4 => BitmapDecoder.Decode(jpeg4),
                _ => null
            };
        }
        catch (SwfException exception)
        {
            _options.Diagnostics?.Report(new RenderDiagnostic(RenderSeverity.Warning, $"Failed to decode image character: {exception.Message}"));
            return null;
        }
    }
}
