package embed_classes
{ 
    import flash.text.Font;
    
    [Embed(
    source="../../assets/font.ttf",
    fontFamily="Great Vibes",
    fontWeight="normal",
    fontStyle="normal",
    mimeType="application/x-font-truetype",
    unicodeRange="U+0020,U+0041-005A", 
	advancedAntiAliasing="true",
    embedAsCFF="false"
    )]
    public class TestFont extends Font
    {
    } 
}
