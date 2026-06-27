using System.Globalization;
using System.Text;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1;

public sealed class Avm1Machine
{
    private readonly byte _version;

    private readonly Stack<Avm1Value> _stack = new();

    private readonly Avm1Object _globals = new();

    private readonly HashSet<ActionOpcode> _unsupported = [];

    private IReadOnlyList<string> _constantPool = [];

    public Avm1Object Globals => _globals;

    public IReadOnlySet<ActionOpcode> UnsupportedOpcodes => _unsupported;

    public Avm1Machine(byte swfVersion = 6)
    {
        _version = swfVersion;
    }

    public static Avm1Object Run(ReadOnlyMemory<byte> actions, byte swfVersion, bool strict = false)
    {
        var machine = new Avm1Machine(swfVersion);
        machine.Execute(Action.DecodeCollection(actions, swfVersion), strict);
        return machine.Globals;
    }

    public void Execute(IReadOnlyList<Action> actions, bool strict = false)
    {
        foreach (var action in actions)
        {
            switch (action)
            {
                case ActionConstantPool constantPool:
                    _constantPool = constantPool.Constants;
                    break;

                case ActionPush push:
                    foreach (var value in push.PushValues)
                        _stack.Push(Resolve(value));
                    break;

                case ActionGetVariable:
                    {
                        var name = ToStr(Pop());
                        _stack.Push(_globals.Members.GetValueOrDefault(name, Avm1Value.Undefined));
                        break;
                    }

                case ActionSetVariable:
                    {
                        var value = Pop();
                        _globals.Members[ToStr(Pop())] = value;
                        break;
                    }

                case ActionGetMember:
                    {
                        var name = ToStr(Pop());
                        _stack.Push(Pop() is Avm1Object members && members.Members.TryGetValue(name, out var value) ? value : Avm1Value.Undefined);
                        break;
                    }

                case ActionSetMember:
                    {
                        var value = Pop();
                        var name = ToStr(Pop());
                        if (Pop() is Avm1Object members)
                            members.Members[name] = value;
                        break;
                    }

                case ActionInitObject:
                    {
                        var count = (int)ToNum(Pop());
                        var members = new Avm1Object();
                        for (var i = 0; i < count; i++)
                        {
                            var value = Pop();
                            members.Members[ToStr(Pop())] = value;
                        }

                        _stack.Push(members);
                        break;
                    }

                case ActionInitArray:
                    {
                        var count = (int)ToNum(Pop());
                        var items = new Avm1Array();
                        for (var i = 0; i < count; i++)
                            items.Items.Add(Pop());

                        _stack.Push(items);
                        break;
                    }

                case ActionNewObject:
                    {
                        _ = ToStr(Pop());
                        DropArguments();
                        _stack.Push(new Avm1Object());
                        break;
                    }

                case ActionCallMethod:
                    {
                        _ = ToStr(Pop());
                        _ = Pop();
                        DropArguments();
                        _stack.Push(Avm1Value.Undefined);
                        break;
                    }

                case ActionCallFunction:
                    {
                        _ = ToStr(Pop());
                        DropArguments();
                        _stack.Push(Avm1Value.Undefined);
                        break;
                    }

                case ActionAdd2:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(left is Avm1String || right is Avm1String
                            ? (Avm1Value)(ToStr(left) + ToStr(right))
                            : ToNum(left) + ToNum(right));
                        break;
                    }

                case ActionStringAdd:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(ToStr(left) + ToStr(right));
                        break;
                    }

                case ActionPop:
                    Pop();
                    break;

                case ActionEnd:
                    return;

                default:
                    if (strict)
                        throw new Avm1UnsupportedActionException(action.Opcode);
                    _unsupported.Add(action.Opcode);
                    break;
            }
        }
    }

    private Avm1Value Resolve(PushValue value)
    {
        return value switch
        {
            PushValue.PushValueString item => item.Value,
            PushValue.PushValueInteger item => item.Value,
            PushValue.PushValueFloat item => (double)item.Value,
            PushValue.PushValueDouble item => item.Value,
            PushValue.PushValueBoolean item => item.Value,
            PushValue.PushValueNull => Avm1Value.Null,
            PushValue.PushValueConstant8 item => Constant(item.ConstantIndex),
            PushValue.PushValueConstant16 item => Constant(item.ConstantIndex),
            _ => Avm1Value.Undefined
        };
    }

    private Avm1Value Constant(int index)
    {
        return index >= 0 && index < _constantPool.Count ? _constantPool[index] : Avm1Value.Undefined;
    }

    private Avm1Value Pop()
    {
        return _stack.Count > 0 ? _stack.Pop() : Avm1Value.Undefined;
    }

    private void DropArguments()
    {
        var count = (int)ToNum(Pop());
        for (var i = 0; i < count; i++)
            Pop();
    }

    internal double ToNum(Avm1Value value)
    {
        return value switch
        {
            Avm1Number number => number.Value,
            Avm1Boolean flag => flag.Value ? 1.0 : 0.0,
            Avm1Null => _version < 7 ? 0.0 : double.NaN,
            Avm1Undefined => _version < 7 ? 0.0 : double.NaN,
            Avm1String text => StringToNumber(text.Value),
            _ => double.NaN
        };
    }

    internal string ToStr(Avm1Value value)
    {
        return value switch
        {
            Avm1String text => text.Value,
            Avm1Number number => FormatNumber(number.Value),
            Avm1Null => "null",
            Avm1Undefined => _version < 7 ? string.Empty : "undefined",
            Avm1Boolean flag => _version < 5 ? (flag.Value ? "1" : "0") : (flag.Value ? "true" : "false"),
            Avm1Array array => string.Join(",", array.Items.Select(ToStr)),
            Avm1Object => "[object Object]",
            _ => "null"
        };
    }

    internal static string FormatNumber(double n)
    {
        if (double.IsNaN(n))
            return "NaN";
        if (double.IsPositiveInfinity(n))
            return "Infinity";
        if (double.IsNegativeInfinity(n))
            return "-Infinity";
        if (n == 0.0)
            return "0";
        if (n >= -2147483648.0 && n <= 2147483647.0 && n == Math.Truncate(n))
            return ((int)n).ToString(CultureInfo.InvariantCulture);

        var buf = new List<char>(25);
        var isNegative = false;
        if (n < 0.0)
        {
            n = -n;
            buf.Add('-');
            isNegative = true;
        }

        const ulong mantissaBits = 52;
        const ulong exponentMask = 0x7ff;
        const int exponentBias = 1023;
        var expBase2 = (int)(((ulong)BitConverter.DoubleToInt64Bits(n) >> (int)mantissaBits) & exponentMask) - exponentBias;

        if (expBase2 == -exponentBias)
        {
            const double normalScale = 1.801439850948198e16;
            var scaled = n * normalScale;
            expBase2 = (int)(((ulong)BitConverter.DoubleToInt64Bits(scaled) >> (int)mantissaBits) & exponentMask) - exponentBias - 54;
        }

        const double log10Of2 = 0.301029995663981;
        var exp = (int)Math.Round(expBase2 * log10Of2, MidpointRounding.AwayFromZero);

        var mantissa = DecimalShift(n, -exp);

        if ((int)mantissa == 0)
        {
            exp -= 1;
            mantissa = DecimalShift(n, -exp);
        }
        if ((int)mantissa >= 10)
        {
            exp += 1;
            mantissa = DecimalShift(n, -exp);
        }

        char Digit()
        {
            var digit = (int)mantissa;
            mantissa -= digit;
            mantissa *= 10.0;
            return (char)('0' + digit);
        }

        const int maxDecimalPlaces = 15;
        if (exp >= 15)
        {
            buf.Add(Digit());
            buf.Add('.');
            for (var i = 0; i < maxDecimalPlaces - 1; i++)
                buf.Add(Digit());
        }
        else if (exp >= 0)
        {
            buf.Add('0');
            for (var i = 0; i <= exp; i++)
                buf.Add(Digit());
            buf.Add('.');
            for (var i = 0; i < maxDecimalPlaces - exp - 1; i++)
                buf.Add(Digit());
            exp = 0;
        }
        else if (exp >= -5)
        {
            buf.Add('0');
            buf.Add('0');
            buf.Add('.');
            for (var i = 0; i < (-exp) - 1; i++)
                buf.Add('0');
            for (var i = 0; i < maxDecimalPlaces; i++)
                buf.Add(Digit());
            exp = 0;
        }
        else
        {
            buf.Add('0');
            var first = Digit();
            if (first != '0')
                buf.Add(first);
            buf.Add('.');
            for (var i = 0; i < maxDecimalPlaces - 1; i++)
                buf.Add(Digit());
        }

        if (Digit() >= '5')
        {
            for (var i = buf.Count - 1; i >= 0; i--)
            {
                if (buf[i] == '9')
                {
                    buf[i] = '0';
                }
                else if (buf[i] >= '0')
                {
                    buf[i] = (char)(buf[i] + 1);
                    break;
                }
            }
        }

        while (buf.Count > 0 && buf[^1] == '0')
            buf.RemoveAt(buf.Count - 1);
        if (buf.Count > 0 && buf[^1] == '.')
            buf.RemoveAt(buf.Count - 1);

        var start = 0;
        if (exp != 0)
        {
            var pos = 0;
            while (pos < buf.Count && buf[pos] == '0')
                pos++;
            if (pos != 0)
            {
                buf.RemoveRange(0, pos);
            }
            if (buf.Count == 0)
            {
                buf.Add('1');
                exp += 1;
            }
            else
            {
                var lastNonZero = -1;
                for (var i = buf.Count - 1; i >= 0; i--)
                {
                    if (buf[i] != '0')
                    {
                        lastNonZero = i;
                        break;
                    }
                }
                if (lastNonZero < 0)
                    lastNonZero = 0;
                if (lastNonZero == 0)
                {
                    exp += buf.Count - 1;
                    buf.RemoveRange(1, buf.Count - 1);
                }
            }

            buf.Add('e');
            buf.Add(exp >= 0 ? '+' : '-');
            foreach (var c in Math.Abs(exp).ToString(CultureInfo.InvariantCulture))
                buf.Add(c);
        }

        var i2 = isNegative ? 1 : 0;
        if (i2 < buf.Count && buf[i2] == '0' && (i2 + 1 >= buf.Count || buf[i2 + 1] != '.'))
        {
            if (i2 > 0)
                buf[i2] = buf[i2 - 1];
            start = 1;
        }

        var sb = new StringBuilder(buf.Count - start);
        for (var i = start; i < buf.Count; i++)
            sb.Append(buf[i]);
        return sb.ToString();
    }

    private static double DecimalShift(double value, int exp)
    {
        var @base = 10.0;
        if (exp > 0)
        {
            while (exp > 0)
            {
                if ((exp & 1) != 0)
                    value *= @base;
                exp >>= 1;
                @base *= @base;
            }
        }
        else
        {
            var magnitude = (uint)Math.Abs((long)exp);
            while (magnitude > 0)
            {
                if ((magnitude & 1) != 0)
                    value /= @base;
                magnitude >>= 1;
                @base *= @base;
            }
        }

        return value;
    }

    private double StringToNumber(string text)
    {
        var s = text.AsSpan().Trim();

        if (_version >= 6)
        {
            var radix = GuessRadix(s);
            if (radix != 10)
            {
                var digits = s;
                if (radix == 16)
                {
                    if (digits.Length < 2)
                        return double.NaN;
                    digits = digits[2..];
                }

                return ParseRadix(digits, radix);
            }
        }

        var strict = _version >= 5;
        var result = ParseFloatImpl(s, strict);
        if (!strict && double.IsNaN(result))
            return 0.0;

        return result;
    }

    private static int GuessRadix(ReadOnlySpan<char> s)
    {
        if (s.Length > 0 && (s[0] == '+' || s[0] == '-'))
            s = s[1..];

        if (s.Length > 0 && s[0] == '0')
        {
            var rest = s[1..];
            if (rest.Length > 0 && (rest[0] == 'x' || rest[0] == 'X'))
                return 16;

            var allOctal = true;
            foreach (var c in rest)
            {
                if (c < '0' || c > '7')
                {
                    allOctal = false;
                    break;
                }
            }
            if (allOctal)
                return 8;
        }

        return 10;
    }

    private static double ParseRadix(ReadOnlySpan<char> s, int radix)
    {
        if (s.Length == 0)
            return double.NaN;

        var negative = false;
        var index = 0;
        if (s[0] == '+' || s[0] == '-')
        {
            negative = s[0] == '-';
            index = 1;
        }

        if (index >= s.Length)
            return double.NaN;

        var value = 0;
        for (; index < s.Length; index++)
        {
            var digit = DigitValue(s[index]);
            if (digit < 0 || digit >= radix)
                return double.NaN;
            value = unchecked(value * radix + digit);
        }

        if (negative)
            value = unchecked(-value);

        return value;
    }

    private static int DigitValue(char c)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'a' && c <= 'z')
            return c - 'a' + 10;
        if (c >= 'A' && c <= 'Z')
            return c - 'A' + 10;
        return -1;
    }

    private static double ParseFloatImpl(ReadOnlySpan<char> s, bool strict)
    {
        s = s.TrimStart();

        var isNegative = false;
        if (s.Length > 0 && (s[0] == '-' || s[0] == '+'))
        {
            isNegative = s[0] == '-';
            s = s[1..];
        }

        var afterSign = s;

        var digitsBefore = 0;
        while (digitsBefore < s.Length && IsAsciiDigit(s[digitsBefore]))
            digitsBefore++;
        s = s[digitsBefore..];
        var exp = digitsBefore - 1;

        if (s.Length > 0 && s[0] == '.')
        {
            s = s[1..];
            var digitsAfter = 0;
            while (digitsAfter < s.Length && IsAsciiDigit(s[digitsAfter]))
                digitsAfter++;
            s = s[digitsAfter..];
        }

        if (s.Length == afterSign.Length)
            return double.NaN;

        if (s.Length > 0 && (s[0] == 'e' || s[0] == 'E'))
        {
            s = s[1..];

            var exponentIsNegative = false;
            if (s.Length > 0 && (s[0] == '-' || s[0] == '+'))
            {
                exponentIsNegative = s[0] == '-';
                s = s[1..];
            }

            var exponent = 0;
            while (s.Length > 0 && IsAsciiDigit(s[0]))
            {
                exponent = unchecked(exponent * 10);
                exponent = unchecked(exponent + (s[0] - '0'));
                s = s[1..];
            }

            if (exponentIsNegative)
                exponent = unchecked(-exponent);

            exp = unchecked(exp + exponent);
        }

        if (strict && s.Length > 0)
            return double.NaN;

        var result = 0.0;
        foreach (var c in afterSign)
        {
            if (IsAsciiDigit(c))
            {
                result += DecimalShift(c - '0', exp);
                exp = unchecked(exp - 1);
            }
            else if (c == '.')
            {
            }
            else
            {
                break;
            }
        }

        if (isNegative)
            result = -result;

        return result;
    }

    private static bool IsAsciiDigit(char c)
    {
        return c >= '0' && c <= '9';
    }
}
