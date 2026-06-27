using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Rendering.Diagnostics;
using ShockwaveFlash.Rendering.Exceptions;
using ShockwaveFlash.Rendering.Model;
using ShockwaveFlash.Rendering.Model.Buttons;
using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Morph;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Model.Sprites;
using ShockwaveFlash.Rendering.Model.Text;
using ShockwaveFlash.Rendering.Processing;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.Bitmap;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Tags.Button;
using ShockwaveFlash.Tags.Font;
using ShockwaveFlash.Tags.Morph;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Tags.Sprite;
using ShockwaveFlash.Tags.Text;
using ShockwaveFlash.Types.Font;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Rendering;

public sealed class SwfRenderer : IImageResolver, ICharacterResolver, IFontResolver
{
    private readonly ShockwaveFlashFile _file;

    private readonly RenderOptions _options;

    private readonly ShapeProcessor _shapeProcessor;

    private readonly TimelineProcessor _timelineProcessor;

    private readonly TextProcessor _textProcessor;

    private readonly EditTextProcessor _editTextProcessor;

    private readonly Dictionary<int, IImage?> _imageCache = new();

    private readonly Dictionary<int, TextDefinition> _textCache = new();

    private readonly Dictionary<int, ButtonDefinition> _buttonCache = new();

    private Dictionary<int, Tag>? _imageTags;

    private Dictionary<int, ShapeDefinition>? _shapes;

    private Dictionary<int, SpriteDefinition>? _sprites;

    private Dictionary<int, Tag>? _textTags;

    private Dictionary<int, Tag>? _buttonTags;

    private Dictionary<int, MorphShapeDefinition>? _morphs;

    private Dictionary<int, ResolvedFont>? _fonts;

    public SwfRenderer(ShockwaveFlashFile file, RenderOptions? options = null)
    {
        _file = file;
        _options = options ?? new RenderOptions();
        _shapeProcessor = new ShapeProcessor(this, _options);
        _timelineProcessor = new TimelineProcessor(this, _options);
        _textProcessor = new TextProcessor(this, _shapeProcessor);
        _editTextProcessor = new EditTextProcessor(this, _shapeProcessor);
    }

    public IDrawable Character(int characterId)
    {
        if (Shape(characterId) is { } shape)
            return shape;

        if (Sprite(characterId) is { } sprite)
            return sprite;

        if (Text(characterId) is { } text)
            return text;

        if (Button(characterId) is { } button)
            return button;

        if (Morph(characterId) is { } morph)
            return morph;

        if (ResolveImage(characterId) is { } image)
            return new ImageDrawable(image);

        Report(RenderSeverity.Warning, $"Character {characterId} is not defined.");

        if (_options.Strict)
            throw new RenderingException($"Character {characterId} is not defined.");

        return MissingCharacter.Instance;
    }

    private void Report(RenderSeverity severity, string message)
    {
        _options.Diagnostics?.Report(new RenderDiagnostic(severity, message));
    }

    private T? Guard<T>(Func<T?> build, string what) where T : class
    {
        try
        {
            return build();
        }
        catch (SwfException exception)
        {
            Report(RenderSeverity.Warning, $"Failed to resolve {what}: {exception.Message}");

            if (_options.Strict)
                throw new RenderingException($"Failed to resolve {what}.", exception);

            return null;
        }
    }

    public MovieDefinition Movie()
    {
        var background = _file.Tags.OfType<SetBackgroundColorTag>().LastOrDefault()?.BackgroundColor;

        return new MovieDefinition(_timelineProcessor, _file.Tags, _file.Header.FrameSize, background);
    }

    public MorphShapeDefinition? Morph(int characterId)
    {
        _morphs ??= IndexMorphs();
        return _morphs.GetValueOrDefault(characterId);
    }

    public ButtonDefinition? Button(int characterId)
    {
        if (_buttonCache.TryGetValue(characterId, out var cached))
            return cached;

        _buttonTags ??= IndexButtonTags();

        var button = _buttonTags.TryGetValue(characterId, out var tag)
            ? Guard(() => tag switch
            {
                DefineButtonTag button1 => new ButtonDefinition(this, button1.Records),
                DefineButton2Tag button2 => new ButtonDefinition(this, button2.Records),
                _ => null
            }, $"button character {characterId}")
            : null;

        if (button is not null)
            _buttonCache[characterId] = button;

        return button;
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

    public TextDefinition? Text(int characterId)
    {
        if (_textCache.TryGetValue(characterId, out var cached))
            return cached;

        _textTags ??= IndexTextTags();

        if (!_textTags.TryGetValue(characterId, out var tag))
            return null;

        var text = Guard(() => tag switch
        {
            DefineTextTag text1 => _textProcessor.Process(text1.Bounds, text1.Matrix, text1.Records),
            DefineEditTextTag editText => _editTextProcessor.Process(editText),
            _ => null
        }, $"text character {characterId}");

        if (text is not null)
            _textCache[characterId] = text;

        return text;
    }

    public ResolvedFont? ResolveFont(int fontId)
    {
        _fonts ??= IndexFonts();
        return _fonts.GetValueOrDefault(fontId);
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

    private Dictionary<int, T> Index<T>(Func<Tag, (int Id, T Value)?> select)
    {
        var map = new Dictionary<int, T>();

        foreach (var tag in _file.Tags)
            if (select(tag) is { } entry)
                map[entry.Id] = entry.Value;

        return map;
    }

    private Dictionary<int, ShapeDefinition> IndexShapes()
    {
        return Index<ShapeDefinition>(tag =>
            tag is DefineShapeTag shape ? (shape.ShapeId, new ShapeDefinition(_shapeProcessor, shape)) : null);
    }

    private Dictionary<int, SpriteDefinition> IndexSprites()
    {
        return Index<SpriteDefinition>(tag =>
            tag is DefineSpriteTag sprite ? (sprite.Id, new SpriteDefinition(_timelineProcessor, sprite)) : null);
    }

    private Dictionary<int, MorphShapeDefinition> IndexMorphs()
    {
        return Index<MorphShapeDefinition>(tag =>
            tag is DefineMorphShapeTag morph ? (morph.Id, new MorphShapeDefinition(_shapeProcessor, morph.Start, morph.End)) : null);
    }

    private Dictionary<int, Tag> IndexTextTags()
    {
        return Index<Tag>(static tag => tag switch
        {
            DefineTextTag text => (text.Id, (Tag)text),
            DefineEditTextTag editText => (editText.Id, editText),
            _ => null
        });
    }

    private Dictionary<int, Tag> IndexButtonTags()
    {
        return Index<Tag>(static tag => tag switch
        {
            DefineButtonTag button1 => (button1.Id, (Tag)button1),
            DefineButton2Tag button2 => (button2.Id, button2),
            _ => null
        });
    }

    private Dictionary<int, ResolvedFont> IndexFonts()
    {
        var codeTables = new Dictionary<int, IReadOnlyList<ushort>>();

        foreach (var tag in _file.Tags)
        {
            switch (tag)
            {
                case DefineFontInfoTag info1:
                    codeTables[info1.Id] = info1.CodeTable;
                    break;

                case DefineFontInfo2Tag info2:
                    codeTables[info2.Id] = info2.CodeTable;
                    break;

                default:
                    break;
            }
        }

        var map = new Dictionary<int, ResolvedFont>();

        foreach (var tag in _file.Tags)
        {
            switch (tag)
            {
                case DefineFontTag font1:
                    map[font1.Id] = BuildFont(BuildGlyphs(font1.Glyphs, codeTables.GetValueOrDefault(font1.Id)), 1024f, null);
                    break;

                case DefineFont3Tag font3:
                    map[font3.Id] = BuildFont(font3.Glyphs, 20480f, font3.Layout);
                    break;

                case DefineFont2Tag font2:
                    map[font2.Id] = BuildFont(font2.Glyphs, 1024f, font2.Layout);
                    break;

                default:
                    break;
            }
        }

        return map;
    }

    private static FontGlyph[] BuildGlyphs(IReadOnlyList<IReadOnlyList<ShapeRecord>> shapes, IReadOnlyList<ushort>? codeTable)
    {
        var glyphs = new FontGlyph[shapes.Count];

        for (var i = 0; i < glyphs.Length; i++)
        {
            var code = codeTable is not null && i < codeTable.Count ? codeTable[i] : (ushort)0;
            glyphs[i] = new FontGlyph(shapes[i], code, advance: 0, bounds: null);
        }

        return glyphs;
    }

    private static ResolvedFont BuildFont(IReadOnlyList<FontGlyph> glyphs, float emSquare, FontLayout? layout)
    {
        return layout is { } resolved
            ? new ResolvedFont(glyphs, emSquare, resolved.Ascent, resolved.Descent, resolved.Leading)
            : new ResolvedFont(glyphs, emSquare, (int)(emSquare * 0.8f), (int)(emSquare * 0.2f), 0);
    }

    private Dictionary<int, Tag> IndexImageTags()
    {
        return Index<Tag>(static tag => tag switch
        {
            DefineBitsLosslessTag lossless => (lossless.Id, (Tag)lossless),
            DefineBitsLossless2Tag lossless2 => (lossless2.Id, lossless2),
            DefineBitsJpeg2Tag jpeg2 => (jpeg2.Id, jpeg2),
            DefineBitsJpeg3Tag jpeg3 => (jpeg3.Id, jpeg3),
            DefineBitsJpeg4Tag jpeg4 => (jpeg4.Id, jpeg4),
            _ => null
        });
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
            Report(RenderSeverity.Warning, $"Failed to decode image character: {exception.Message}");

            if (_options.Strict)
                throw new RenderingException("Failed to decode image character.", exception);

            return null;
        }
    }
}
