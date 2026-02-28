using ShockwaveFlash;
using ShockwaveFlash.IO.Binary;
using ShockwaveFlash.Tags.Action;

var buffer = File.ReadAllBytes(args[0]);

var reader = new SpanReader(buffer);

var swf = ShockwaveFlashFile.Disassemble(ref reader);

foreach (var doActionTag in swf.Tags.OfType<DoActionTag>())
{
    var actions = doActionTag.DecodeActions(ref reader, swf.Header.Version);

    foreach (var action in actions)
        Console.WriteLine(action);
}
