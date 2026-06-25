using ShockwaveFlash;
using ShockwaveFlash.Tags.Action;

var buffer = File.ReadAllBytes(args[0]);

var swf = ShockwaveFlashFile.Disassemble(buffer);

foreach (var doActionTag in swf.Tags.OfType<DoActionTag>())
{
    var actions = doActionTag.DecodeActions(swf.Header.Version);

    foreach (var action in actions)
        Console.WriteLine(action);
}
