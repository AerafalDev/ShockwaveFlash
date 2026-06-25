using ShockwaveFlash;

var buffer = File.ReadAllBytes(args[0]);

var swf = ShockwaveFlashFile.Disassemble(buffer);

Console.WriteLine(swf.Header);

foreach (var tag in swf.Tags)
    Console.WriteLine(tag.Metadata.Code);
