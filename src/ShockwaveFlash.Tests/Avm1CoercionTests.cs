using ShockwaveFlash.Avm1;
using ShockwaveFlash.Avm1.Types;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class Avm1CoercionTests
{
    [Theory]
    [InlineData(0.0, "0")]
    [InlineData(-0.0, "0")]
    [InlineData(1.0, "1")]
    [InlineData(5.0, "5")]
    [InlineData(1.4, "1.4")]
    [InlineData(-990.123, "-990.123")]
    [InlineData(double.NaN, "NaN")]
    [InlineData(double.PositiveInfinity, "Infinity")]
    [InlineData(double.NegativeInfinity, "-Infinity")]
    [InlineData(9.9999e14, "999990000000000")]
    [InlineData(-9.9999e14, "-999990000000000")]
    [InlineData(1e15, "1e+15")]
    [InlineData(-1e15, "-1e+15")]
    [InlineData(1e-5, "0.00001")]
    [InlineData(-1e-5, "-0.00001")]
    [InlineData(0.999e-5, "9.99e-6")]
    [InlineData(-0.999e-5, "-9.99e-6")]
    [InlineData(0.19999999999999996, "0.2")]
    [InlineData(-0.19999999999999996, "-0.2")]
    [InlineData(100000.12345678912, "100000.123456789")]
    [InlineData(-100000.12345678912, "-100000.123456789")]
    [InlineData(0.8000000000000005, "0.800000000000001")]
    [InlineData(-0.8000000000000005, "-0.800000000000001")]
    [InlineData(0.8300000000000005, "0.83")]
    [InlineData(1e-320, "9.99988867182684e-321")]
    [InlineData(double.MinValue, "-1.79769313486231e+308")]
    [InlineData(2.2250738585072014e-308, "2.2250738585072e-308")]
    [InlineData(double.MaxValue, "1.79769313486231e+308")]
    [InlineData(5e-324, "4.94065645841247e-324")]
    [InlineData(9.999999999999999, "10")]
    [InlineData(-9.999999999999999, "-10")]
    [InlineData(9999999999999996.0, "1e+16")]
    [InlineData(-9999999999999996.0, "-e+16")]
    [InlineData(0.000009999999999999996, "1e-5")]
    [InlineData(-0.000009999999999999996, "-10e-6")]
    [InlineData(0.00009999999999999996, "0.0001")]
    [InlineData(-0.00009999999999999996, "-0.0001")]
    public void FormatNumber_matches_flash_oracle(double value, string expected)
    {
        Avm1Machine.FormatNumber(value).ShouldBe(expected);
    }

    [Fact]
    public void ToStr_undefined_is_empty_before_swf7()
    {
        var machine = new Avm1Machine(6);

        machine.ToStr(Avm1Value.Undefined).ShouldBe(string.Empty);
    }

    [Fact]
    public void ToStr_undefined_is_undefined_from_swf7()
    {
        var machine = new Avm1Machine(7);

        machine.ToStr(Avm1Value.Undefined).ShouldBe("undefined");
    }

    [Fact]
    public void ToStr_null_is_null()
    {
        var machine = new Avm1Machine(6);

        machine.ToStr(Avm1Value.Null).ShouldBe("null");
    }

    [Fact]
    public void ToStr_boolean_is_numeric_before_swf5()
    {
        var machine = new Avm1Machine(4);

        machine.ToStr(new Avm1Boolean(true)).ShouldBe("1");
        machine.ToStr(new Avm1Boolean(false)).ShouldBe("0");
    }

    [Fact]
    public void ToStr_boolean_is_word_from_swf5()
    {
        var machine = new Avm1Machine(5);

        machine.ToStr(new Avm1Boolean(true)).ShouldBe("true");
        machine.ToStr(new Avm1Boolean(false)).ShouldBe("false");
    }

    [Fact]
    public void ToStr_object_is_object_object()
    {
        var machine = new Avm1Machine(6);

        machine.ToStr(new Avm1Object()).ShouldBe("[object Object]");
    }

    [Fact]
    public void ToStr_array_joins_items_with_commas()
    {
        var machine = new Avm1Machine(6);
        var array = new Avm1Array();
        array.Items.Add(new Avm1Number(10));
        array.Items.Add(new Avm1String("a"));
        array.Items.Add(new Avm1Boolean(true));

        machine.ToStr(array).ShouldBe("10,a,true");
    }

    [Fact]
    public void ToNum_undefined_is_zero_before_swf7()
    {
        var machine = new Avm1Machine(6);

        machine.ToNum(Avm1Value.Undefined).ShouldBe(0.0);
    }

    [Fact]
    public void ToNum_undefined_is_nan_from_swf7()
    {
        var machine = new Avm1Machine(7);

        double.IsNaN(machine.ToNum(Avm1Value.Undefined)).ShouldBeTrue();
    }

    [Fact]
    public void ToNum_null_is_zero_before_swf7()
    {
        var machine = new Avm1Machine(6);

        machine.ToNum(Avm1Value.Null).ShouldBe(0.0);
    }

    [Fact]
    public void ToNum_null_is_nan_from_swf7()
    {
        var machine = new Avm1Machine(7);

        double.IsNaN(machine.ToNum(Avm1Value.Null)).ShouldBeTrue();
    }

    [Fact]
    public void ToNum_boolean_is_one_or_zero()
    {
        var machine = new Avm1Machine(6);

        machine.ToNum(new Avm1Boolean(true)).ShouldBe(1.0);
        machine.ToNum(new Avm1Boolean(false)).ShouldBe(0.0);
    }

    [Fact]
    public void ToNum_object_is_nan()
    {
        var machine = new Avm1Machine(6);

        double.IsNaN(machine.ToNum(new Avm1Object())).ShouldBeTrue();
    }

    [Fact]
    public void ToNum_string_non_numeric_is_zero_before_swf5()
    {
        var machine = new Avm1Machine(4);

        machine.ToNum(new Avm1String("abc")).ShouldBe(0.0);
    }

    [Fact]
    public void ToNum_string_non_numeric_is_nan_from_swf5()
    {
        var machine = new Avm1Machine(5);

        double.IsNaN(machine.ToNum(new Avm1String("abc"))).ShouldBeTrue();
    }

    [Fact]
    public void ToNum_string_decimal_parses()
    {
        var machine = new Avm1Machine(6);

        machine.ToNum(new Avm1String("12.5")).ShouldBe(12.5);
    }

    [Fact]
    public void ToNum_string_hex_parses_from_swf6()
    {
        var machine = new Avm1Machine(6);

        machine.ToNum(new Avm1String("0x10")).ShouldBe(16.0);
    }
}
