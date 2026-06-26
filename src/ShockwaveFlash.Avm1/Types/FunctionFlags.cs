namespace ShockwaveFlash.Avm1.Types;

[Flags]
public enum FunctionFlags : ushort
{
    PreloadThis = 1 << 0,
    SuppressThis = 1 << 1,
    PreloadArguments = 1 << 2,
    SuppressArguments = 1 << 3,
    PreloadSuper = 1 << 4,
    SuppressSuper = 1 << 5,
    PreloadRoot = 1 << 6,
    PreloadParent = 1 << 7,
    PreloadGlobal = 1 << 8
}
