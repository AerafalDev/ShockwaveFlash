using ShockwaveFlash.Avm1;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Types;
using Shouldly;
using Avm1Action = ShockwaveFlash.Avm1.Action;

namespace ShockwaveFlash.Tests;

public sealed class Avm1ConversionCallTests
{
    private static Avm1Object Run(byte version, IEnumerable<Avm1Action> statements)
    {
        var actions = new List<Avm1Action>(statements) { new ActionEnd() };
        return Avm1Machine.Run(Avm1Action.EncodeCollection(actions, version), version, strict: true);
    }

    private static IEnumerable<Avm1Action> Call(string function, params PushValue[] arguments)
    {
        foreach (var argument in Enumerable.Reverse(arguments))
            yield return new ActionPush([argument]);

        yield return new ActionPush([PushValue.Integer(arguments.Length), PushValue.String(function)]);
        yield return new ActionCallFunction();
    }

    private static Avm1Value Evaluate(byte version, string function, params PushValue[] arguments)
    {
        var statement = new List<Avm1Action> { new ActionPush([PushValue.String("x")]) };
        statement.AddRange(Call(function, arguments));
        statement.Add(new ActionSetVariable());

        return Run(version, statement)["x"];
    }

    private static IEnumerable<Avm1Action> SetConstant(string name, string function, PushValue argument)
    {
        yield return new ActionPush([PushValue.String("C")]);
        yield return new ActionGetVariable();
        yield return new ActionPush([PushValue.String(name)]);
        foreach (var action in Call(function, argument))
            yield return action;
        yield return new ActionSetMember();
    }

    [Fact]
    public void Lang_constants_assigned_through_conversion_calls_are_evaluated()
    {
        var statements = new List<Avm1Action>
        {
            new ActionPush([PushValue.String("C"), PushValue.Integer(0)]),
            new ActionInitObject(),
            new ActionSetVariable(),
        };
        statements.AddRange(SetConstant("CHECK_MAP_FILE_ID", "Boolean", PushValue.Boolean(true)));
        statements.AddRange(SetConstant("SPEAKING_ITEMS_MSG_COUNT", "Number", PushValue.Integer(30)));
        statements.AddRange(SetConstant("RSS_LINK", "String", PushValue.String("http://example.com/rss")));

        var constants = Run(6, statements)["C"].AsObject;

        constants["CHECK_MAP_FILE_ID"].ShouldBe(new Avm1Boolean(true));
        constants["SPEAKING_ITEMS_MSG_COUNT"].ShouldBe(new Avm1Number(30));
        constants["RSS_LINK"].ShouldBe(new Avm1String("http://example.com/rss"));
    }

    [Fact]
    public void String_converts_its_argument()
    {
        Evaluate(6, "String", PushValue.String("text")).ShouldBe(new Avm1String("text"));
        Evaluate(6, "String", PushValue.Double(12.5)).ShouldBe(new Avm1String("12.5"));
        Evaluate(6, "String", PushValue.Boolean(true)).ShouldBe(new Avm1String("true"));
        Evaluate(6, "String", PushValue.Null()).ShouldBe(new Avm1String("null"));
    }

    [Fact]
    public void String_of_undefined_is_version_gated()
    {
        Evaluate(6, "String", PushValue.Undefined()).ShouldBe(new Avm1String(string.Empty));
        Evaluate(7, "String", PushValue.Undefined()).ShouldBe(new Avm1String("undefined"));
    }

    [Fact]
    public void Number_converts_its_argument()
    {
        Evaluate(6, "Number", PushValue.Integer(30)).ShouldBe(new Avm1Number(30));
        Evaluate(6, "Number", PushValue.String("30")).ShouldBe(new Avm1Number(30));
        Evaluate(6, "Number", PushValue.String("0x1F")).ShouldBe(new Avm1Number(31));
        Evaluate(6, "Number", PushValue.Boolean(true)).ShouldBe(new Avm1Number(1));
        Evaluate(6, "Number", PushValue.String("abc")).AsNumber.ShouldBe(double.NaN);
    }

    [Fact]
    public void Boolean_converts_its_argument()
    {
        Evaluate(6, "Boolean", PushValue.Boolean(true)).ShouldBe(new Avm1Boolean(true));
        Evaluate(6, "Boolean", PushValue.Integer(0)).ShouldBe(new Avm1Boolean(false));
        Evaluate(6, "Boolean", PushValue.Integer(5)).ShouldBe(new Avm1Boolean(true));
        Evaluate(6, "Boolean", PushValue.Null()).ShouldBe(new Avm1Boolean(false));
    }

    [Fact]
    public void Boolean_of_a_string_is_version_gated()
    {
        Evaluate(6, "Boolean", PushValue.String("0")).ShouldBe(new Avm1Boolean(false));
        Evaluate(7, "Boolean", PushValue.String("0")).ShouldBe(new Avm1Boolean(true));
    }

    [Fact]
    public void Calls_without_arguments_return_the_player_defaults()
    {
        Evaluate(6, "String").ShouldBe(new Avm1String(string.Empty));
        Evaluate(6, "Number").ShouldBe(new Avm1Number(0));
        Evaluate(6, "Boolean").ShouldBe(Avm1Value.Undefined);
    }

    [Fact]
    public void Extra_arguments_are_ignored()
    {
        Evaluate(6, "Number", PushValue.Integer(1), PushValue.Integer(2), PushValue.Integer(3)).ShouldBe(new Avm1Number(1));
    }

    [Fact]
    public void Function_names_are_case_insensitive_before_version_7()
    {
        Evaluate(6, "number", PushValue.String("5")).ShouldBe(new Avm1Number(5));
        Evaluate(7, "number", PushValue.String("5")).ShouldBe(Avm1Value.Undefined);
    }

    [Fact]
    public void Other_functions_still_evaluate_to_undefined()
    {
        Evaluate(6, "parseInt", PushValue.String("5")).ShouldBe(Avm1Value.Undefined);
    }
}
