using ShockwaveFlash.Avm1;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Swf6;
using ShockwaveFlash.Avm1.Types;
using Shouldly;
using Avm1Action = ShockwaveFlash.Avm1.Action;

namespace ShockwaveFlash.Tests;

public sealed class Avm1OperatorTests
{
    private static Avm1Value Evaluate(byte version, IEnumerable<Avm1Action> operations)
    {
        var actions = new List<Avm1Action> { new ActionPush([PushValue.String("x")]) };
        actions.AddRange(operations);
        actions.Add(new ActionSetVariable());
        actions.Add(new ActionEnd());

        var machine = new Avm1Machine(version);
        machine.Execute(actions);
        return machine.Globals["x"];
    }

    private static double Number(byte version, IEnumerable<Avm1Action> operations)
    {
        return Evaluate(version, operations).AsNumber;
    }

    private static bool Boolean(byte version, IEnumerable<Avm1Action> operations)
    {
        return Evaluate(version, operations).AsBoolean;
    }

    private static string Text(byte version, IEnumerable<Avm1Action> operations)
    {
        return Evaluate(version, operations).AsString;
    }

    [Fact]
    public void Add_sums_numbers()
    {
        Number(6, [new ActionPush([PushValue.Integer(7)]), new ActionPush([PushValue.Integer(3)]), new ActionAdd()]).ShouldBe(10d);
    }

    [Fact]
    public void Subtract_uses_left_minus_right()
    {
        Number(6, [new ActionPush([PushValue.Integer(7)]), new ActionPush([PushValue.Integer(3)]), new ActionSubtract()]).ShouldBe(4d);
    }

    [Fact]
    public void Multiply_multiplies()
    {
        Number(6, [new ActionPush([PushValue.Integer(7)]), new ActionPush([PushValue.Integer(3)]), new ActionMultiply()]).ShouldBe(21d);
    }

    [Fact]
    public void Divide_by_zero_is_infinity()
    {
        Number(6, [new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.Integer(0)]), new ActionDivide()]).ShouldBe(double.PositiveInfinity);
    }

    [Fact]
    public void Modulo_uses_double_remainder()
    {
        Number(6, [new ActionPush([PushValue.Integer(7)]), new ActionPush([PushValue.Integer(3)]), new ActionModulo()]).ShouldBe(1d);
    }

    [Fact]
    public void BitAnd_masks_bits()
    {
        Number(6, [new ActionPush([PushValue.Integer(6)]), new ActionPush([PushValue.Integer(3)]), new ActionBitAnd()]).ShouldBe(2d);
    }

    [Fact]
    public void BitOr_combines_bits()
    {
        Number(6, [new ActionPush([PushValue.Integer(6)]), new ActionPush([PushValue.Integer(1)]), new ActionBitOr()]).ShouldBe(7d);
    }

    [Fact]
    public void BitXor_toggles_bits()
    {
        Number(6, [new ActionPush([PushValue.Integer(6)]), new ActionPush([PushValue.Integer(3)]), new ActionBitXor()]).ShouldBe(5d);
    }

    [Fact]
    public void BitLShift_shifts_left()
    {
        Number(6, [new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.Integer(4)]), new ActionBitLShift()]).ShouldBe(16d);
    }

    [Fact]
    public void BitRShift_keeps_sign()
    {
        Number(6, [new ActionPush([PushValue.Integer(-16)]), new ActionPush([PushValue.Integer(2)]), new ActionBitRShift()]).ShouldBe(-4d);
    }

    [Fact]
    public void BitURShift_produces_unsigned_result()
    {
        Number(6, [new ActionPush([PushValue.Integer(-1)]), new ActionPush([PushValue.Integer(0)]), new ActionBitURShift()]).ShouldBe(4294967295d);
    }

    [Fact]
    public void Less2_compares_numeric_values()
    {
        Boolean(6, [new ActionPush([PushValue.Integer(3)]), new ActionPush([PushValue.Integer(5)]), new ActionLess2()]).ShouldBeTrue();
        Boolean(6, [new ActionPush([PushValue.Integer(5)]), new ActionPush([PushValue.Integer(3)]), new ActionLess2()]).ShouldBeFalse();
    }

    [Fact]
    public void Less2_compares_strings_lexically()
    {
        Boolean(6, [new ActionPush([PushValue.String("a")]), new ActionPush([PushValue.String("b")]), new ActionLess2()]).ShouldBeTrue();
    }

    [Fact]
    public void Greater_is_swapped_abstract_less()
    {
        Boolean(6, [new ActionPush([PushValue.Integer(5)]), new ActionPush([PushValue.Integer(3)]), new ActionGreater()]).ShouldBeTrue();
    }

    [Fact]
    public void Equals2_coerces_mixed_types()
    {
        Boolean(6, [new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.String("1")]), new ActionEquals2()]).ShouldBeTrue();
        Boolean(6, [new ActionPush([PushValue.Boolean(true)]), new ActionPush([PushValue.Integer(1)]), new ActionEquals2()]).ShouldBeTrue();
    }

    [Fact]
    public void Equals2_treats_null_and_undefined_as_equal()
    {
        Boolean(6, [new ActionPush([PushValue.Null()]), new ActionPush([PushValue.Undefined()]), new ActionEquals2()]).ShouldBeTrue();
    }

    [Fact]
    public void StrictEquals_requires_matching_type()
    {
        Boolean(6, [new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.String("1")]), new ActionStrictEquals()]).ShouldBeFalse();
        Boolean(6, [new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.Integer(1)]), new ActionStrictEquals()]).ShouldBeTrue();
    }

    [Fact]
    public void Equals_swf4_pushes_boolean()
    {
        Boolean(4, [new ActionPush([PushValue.Integer(2)]), new ActionPush([PushValue.Integer(2)]), new ActionEquals()]).ShouldBeTrue();
    }

    [Fact]
    public void Less_swf4_compares_numbers()
    {
        Boolean(4, [new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.Integer(2)]), new ActionLess()]).ShouldBeTrue();
    }

    [Fact]
    public void Not_inverts_truthiness()
    {
        Boolean(6, [new ActionPush([PushValue.Integer(0)]), new ActionNot()]).ShouldBeTrue();
        Boolean(6, [new ActionPush([PushValue.Integer(1)]), new ActionNot()]).ShouldBeFalse();
    }

    [Fact]
    public void And_requires_both_operands()
    {
        Boolean(6, [new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.Integer(1)]), new ActionAnd()]).ShouldBeTrue();
        Boolean(6, [new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.Integer(0)]), new ActionAnd()]).ShouldBeFalse();
    }

    [Fact]
    public void Or_accepts_either_operand()
    {
        Boolean(6, [new ActionPush([PushValue.Integer(0)]), new ActionPush([PushValue.Integer(1)]), new ActionOr()]).ShouldBeTrue();
        Boolean(6, [new ActionPush([PushValue.Integer(0)]), new ActionPush([PushValue.Integer(0)]), new ActionOr()]).ShouldBeFalse();
    }

    [Fact]
    public void StringEquals_compares_text()
    {
        Boolean(6, [new ActionPush([PushValue.String("ab")]), new ActionPush([PushValue.String("ab")]), new ActionStringEquals()]).ShouldBeTrue();
    }

    [Fact]
    public void StringLength_counts_characters()
    {
        Number(6, [new ActionPush([PushValue.String("hello")]), new ActionStringLength()]).ShouldBe(5d);
    }

    [Fact]
    public void StringExtract_takes_a_one_based_substring()
    {
        Text(6, [new ActionPush([PushValue.String("hello")]), new ActionPush([PushValue.Integer(2)]), new ActionPush([PushValue.Integer(3)]), new ActionStringExtract()]).ShouldBe("ell");
    }

    [Fact]
    public void StringExtract_clamps_overlong_count()
    {
        Text(6, [new ActionPush([PushValue.String("hi")]), new ActionPush([PushValue.Integer(1)]), new ActionPush([PushValue.Integer(99)]), new ActionStringExtract()]).ShouldBe("hi");
    }

    [Fact]
    public void StringLess_and_greater_compare_lexically()
    {
        Boolean(6, [new ActionPush([PushValue.String("a")]), new ActionPush([PushValue.String("b")]), new ActionStringLess()]).ShouldBeTrue();
        Boolean(6, [new ActionPush([PushValue.String("b")]), new ActionPush([PushValue.String("a")]), new ActionStringGreater()]).ShouldBeTrue();
    }

    [Fact]
    public void CharToAscii_returns_first_code_unit()
    {
        Number(6, [new ActionPush([PushValue.String("A")]), new ActionCharToAscii()]).ShouldBe(65d);
    }

    [Fact]
    public void AsciiToChar_builds_a_single_character()
    {
        Text(6, [new ActionPush([PushValue.Integer(65)]), new ActionAsciiToChar()]).ShouldBe("A");
    }

    [Fact]
    public void MbStringLength_matches_string_length()
    {
        Number(6, [new ActionPush([PushValue.String("abcd")]), new ActionMBStringLength()]).ShouldBe(4d);
    }

    [Fact]
    public void ToInteger_truncates_toward_zero()
    {
        Number(6, [new ActionPush([PushValue.Double(3.9)]), new ActionToInteger()]).ShouldBe(3d);
        Number(6, [new ActionPush([PushValue.Double(-3.9)]), new ActionToInteger()]).ShouldBe(-3d);
    }

    [Fact]
    public void ToNumber_parses_a_string()
    {
        Number(6, [new ActionPush([PushValue.String("12.5")]), new ActionToNumber()]).ShouldBe(12.5);
    }

    [Fact]
    public void ToString_formats_a_number()
    {
        Text(6, [new ActionPush([PushValue.Integer(42)]), new ActionToString()]).ShouldBe("42");
    }

    [Fact]
    public void TypeOf_number()
    {
        Text(6, [new ActionPush([PushValue.Integer(1)]), new ActionTypeOf()]).ShouldBe("number");
    }

    [Fact]
    public void TypeOf_string()
    {
        Text(6, [new ActionPush([PushValue.String("a")]), new ActionTypeOf()]).ShouldBe("string");
    }

    [Fact]
    public void TypeOf_boolean()
    {
        Text(6, [new ActionPush([PushValue.Boolean(true)]), new ActionTypeOf()]).ShouldBe("boolean");
    }

    [Fact]
    public void TypeOf_null()
    {
        Text(6, [new ActionPush([PushValue.Null()]), new ActionTypeOf()]).ShouldBe("null");
    }

    [Fact]
    public void TypeOf_undefined()
    {
        Text(6, [new ActionPush([PushValue.Undefined()]), new ActionTypeOf()]).ShouldBe("undefined");
    }

    [Fact]
    public void TypeOf_object()
    {
        var machine = new Avm1Machine(6);
        machine.Execute(
        [
            new ActionPush([PushValue.String("x")]),
            new ActionPush([PushValue.Integer(0)]),
            new ActionInitObject(),
            new ActionTypeOf(),
            new ActionSetVariable(),
            new ActionEnd(),
        ]);

        machine.Globals["x"].AsString.ShouldBe("object");
    }

    [Fact]
    public void TypeOf_array_is_object()
    {
        var machine = new Avm1Machine(6);
        machine.Execute(
        [
            new ActionPush([PushValue.String("x")]),
            new ActionPush([PushValue.Integer(0)]),
            new ActionInitArray(),
            new ActionTypeOf(),
            new ActionSetVariable(),
            new ActionEnd(),
        ]);

        machine.Globals["x"].AsString.ShouldBe("object");
    }

    [Fact]
    public void PushDuplicate_copies_the_top()
    {
        Number(6, [new ActionPush([PushValue.Integer(9)]), new ActionPushDuplicate(), new ActionAdd()]).ShouldBe(18d);
    }

    [Fact]
    public void StackSwap_exchanges_the_top_two()
    {
        Number(6, [new ActionPush([PushValue.Integer(10)]), new ActionPush([PushValue.Integer(3)]), new ActionStackSwap(), new ActionSubtract()]).ShouldBe(-7d);
    }

    [Fact]
    public void StoreRegister_peeks_and_push_register_resolves()
    {
        var machine = new Avm1Machine(6);
        machine.Execute(
        [
            new ActionPush([PushValue.String("x")]),
            new ActionPush([PushValue.Integer(77)]),
            new ActionStoreRegister(2),
            new ActionPop(),
            new ActionPush([PushValue.Register(2)]),
            new ActionSetVariable(),
            new ActionEnd(),
        ]);

        machine.Globals["x"].AsNumber.ShouldBe(77d);
    }
}
