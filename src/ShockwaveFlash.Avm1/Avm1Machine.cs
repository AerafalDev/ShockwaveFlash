using System.Globalization;
using System.Text;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Swf6;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1;

public sealed class Avm1Machine
{
    private readonly byte _version;

    private readonly Stack<Avm1Value> _stack = new();

    private readonly Avm1Object _globals = new();

    private readonly Avm1Value[] _registers = CreateRegisters();

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

                case ActionAdd:
                    {
                        var right = ToNum(Pop());
                        var left = ToNum(Pop());
                        _stack.Push(left + right);
                        break;
                    }

                case ActionSubtract:
                    {
                        var right = ToNum(Pop());
                        var left = ToNum(Pop());
                        _stack.Push(left - right);
                        break;
                    }

                case ActionMultiply:
                    {
                        var right = ToNum(Pop());
                        var left = ToNum(Pop());
                        _stack.Push(left * right);
                        break;
                    }

                case ActionDivide:
                    {
                        var right = ToNum(Pop());
                        var left = ToNum(Pop());
                        _stack.Push(right == 0.0 && _version < 5 ? "#ERROR#" : left / right);
                        break;
                    }

                case ActionModulo:
                    {
                        var right = ToNum(Pop());
                        var left = ToNum(Pop());
                        _stack.Push(left % right);
                        break;
                    }

                case ActionNot:
                    _stack.Push(!ToBool(Pop()));
                    break;

                case ActionAnd:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(ToBool(left) && ToBool(right));
                        break;
                    }

                case ActionOr:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(ToBool(left) || ToBool(right));
                        break;
                    }

                case ActionBitAnd:
                    {
                        var right = ToInt32(Pop());
                        var left = ToInt32(Pop());
                        _stack.Push((double)(left & right));
                        break;
                    }

                case ActionBitOr:
                    {
                        var right = ToInt32(Pop());
                        var left = ToInt32(Pop());
                        _stack.Push((double)(left | right));
                        break;
                    }

                case ActionBitXor:
                    {
                        var right = ToInt32(Pop());
                        var left = ToInt32(Pop());
                        _stack.Push((double)(left ^ right));
                        break;
                    }

                case ActionBitLShift:
                    {
                        var count = (int)(ToUint32(Pop()) & 31);
                        var left = ToInt32(Pop());
                        _stack.Push((double)(left << count));
                        break;
                    }

                case ActionBitRShift:
                    {
                        var count = (int)(ToUint32(Pop()) & 31);
                        var left = ToInt32(Pop());
                        _stack.Push((double)(left >> count));
                        break;
                    }

                case ActionBitURShift:
                    {
                        var count = (int)(ToUint32(Pop()) & 31);
                        var left = ToUint32(Pop());
                        _stack.Push((double)(left >> count));
                        break;
                    }

                case ActionEquals:
                    {
                        var right = ToNum(Pop());
                        var left = ToNum(Pop());
                        _stack.Push(left == right);
                        break;
                    }

                case ActionLess:
                    {
                        var right = ToNum(Pop());
                        var left = ToNum(Pop());
                        _stack.Push(left < right);
                        break;
                    }

                case ActionEquals2:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(AbstractEquals(left, right));
                        break;
                    }

                case ActionLess2:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(AbstractLess(left, right));
                        break;
                    }

                case ActionGreater:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(AbstractLess(right, left));
                        break;
                    }

                case ActionStrictEquals:
                    {
                        var right = Pop();
                        var left = Pop();
                        _stack.Push(StrictEquals(left, right));
                        break;
                    }

                case ActionStringEquals:
                    {
                        var right = ToStr(Pop());
                        var left = ToStr(Pop());
                        _stack.Push(string.Equals(left, right, StringComparison.Ordinal));
                        break;
                    }

                case ActionStringLength:
                case ActionMBStringLength:
                    _stack.Push((double)ToStr(Pop()).Length);
                    break;

                case ActionStringExtract:
                case ActionMBStringExtract:
                    {
                        var count = ToInt32(Pop());
                        var index = ToInt32(Pop());
                        var text = ToStr(Pop());
                        _stack.Push(StringExtract(text, index, count));
                        break;
                    }

                case ActionStringLess:
                    {
                        var right = ToStr(Pop());
                        var left = ToStr(Pop());
                        _stack.Push(string.CompareOrdinal(left, right) < 0);
                        break;
                    }

                case ActionStringGreater:
                    {
                        var right = ToStr(Pop());
                        var left = ToStr(Pop());
                        _stack.Push(string.CompareOrdinal(left, right) > 0);
                        break;
                    }

                case ActionCharToAscii:
                case ActionMBCharToAscii:
                    {
                        var text = ToStr(Pop());
                        _stack.Push((double)(text.Length > 0 ? text[0] : 0));
                        break;
                    }

                case ActionAsciiToChar:
                case ActionMBAsciiToChar:
                    {
                        var code = (char)(ushort)ToInt32(Pop());
                        _stack.Push(code == '\0' ? string.Empty : code.ToString());
                        break;
                    }

                case ActionToInteger:
                    _stack.Push((double)ToInt32(Pop()));
                    break;

                case ActionToNumber:
                    _stack.Push(ToNum(Pop()));
                    break;

                case ActionToString:
                    _stack.Push(ToStr(Pop()));
                    break;

                case ActionTypeOf:
                    _stack.Push(TypeOf(Pop()));
                    break;

                case ActionPushDuplicate:
                    {
                        var value = Pop();
                        _stack.Push(value);
                        _stack.Push(value);
                        break;
                    }

                case ActionStackSwap:
                    {
                        var top = Pop();
                        var under = Pop();
                        _stack.Push(top);
                        _stack.Push(under);
                        break;
                    }

                case ActionStoreRegister storeRegister:
                    {
                        var value = _stack.Count > 0 ? _stack.Peek() : Avm1Value.Undefined;
                        if (storeRegister.RegisterNumber < _registers.Length)
                            _registers[storeRegister.RegisterNumber] = value;
                        break;
                    }

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
            PushValue.PushValueRegister item => Register(item.RegisterIndex),
            PushValue.PushValueConstant8 item => Constant(item.ConstantIndex),
            PushValue.PushValueConstant16 item => Constant(item.ConstantIndex),
            _ => Avm1Value.Undefined
        };
    }

    private Avm1Value Register(int index)
    {
        return index >= 0 && index < _registers.Length ? _registers[index] : Avm1Value.Undefined;
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

    internal bool ToBool(Avm1Value value)
    {
        return value switch
        {
            Avm1Boolean flag => flag.Value,
            Avm1Number number => !double.IsNaN(number.Value) && number.Value != 0.0,
            Avm1String text => ToBoolString(text.Value),
            Avm1Object => true,
            Avm1Array => true,
            _ => false
        };
    }

    internal int ToInt32(Avm1Value value)
    {
        return unchecked((int)ToUint32(value));
    }

    internal uint ToUint32(Avm1Value value)
    {
        var number = ToNum(value);
        if (!double.IsFinite(number))
            return 0;

        var truncated = Math.Truncate(number);
        var wrapped = truncated % 4294967296.0;
        if (wrapped < 0.0)
            wrapped += 4294967296.0;

        return (uint)wrapped;
    }

    internal static string TypeOf(Avm1Value value)
    {
        return value switch
        {
            Avm1Undefined => "undefined",
            Avm1Null => "null",
            Avm1Number => "number",
            Avm1Boolean => "boolean",
            Avm1String => "string",
            _ => "object"
        };
    }

    internal bool AbstractEquals(Avm1Value left, Avm1Value right)
    {
        if (left is Avm1Object or Avm1Array || right is Avm1Object or Avm1Array)
            return ReferenceEquals(left, right);

        var leftNull = left is Avm1Null or Avm1Undefined;
        var rightNull = right is Avm1Null or Avm1Undefined;
        if (leftNull || rightNull)
            return leftNull && rightNull;

        if (left is Avm1String leftString && right is Avm1String rightString)
            return string.Equals(leftString.Value, rightString.Value, StringComparison.Ordinal);

        if (left is Avm1Boolean && right is Avm1Boolean)
            return ToBool(left) == ToBool(right);

        return ToNum(left) == ToNum(right);
    }

    internal Avm1Value AbstractLess(Avm1Value left, Avm1Value right)
    {
        if (left is Avm1Object or Avm1Array || right is Avm1Object or Avm1Array)
            return false;

        if (left is Avm1String leftString && right is Avm1String rightString)
            return string.CompareOrdinal(leftString.Value, rightString.Value) < 0;

        var leftNumber = ToNum(left);
        var rightNumber = ToNum(right);
        if (double.IsNaN(leftNumber) || double.IsNaN(rightNumber))
            return Avm1Value.Undefined;

        return leftNumber < rightNumber;
    }

    internal static bool StrictEquals(Avm1Value left, Avm1Value right)
    {
        if (left is Avm1Object or Avm1Array || right is Avm1Object or Avm1Array)
            return ReferenceEquals(left, right);

        return left == right;
    }

    internal static string StringExtract(string text, int index, int count)
    {
        var start = index >= 1 ? index - 1 : 0;
        if (start > text.Length)
            start = text.Length;

        var end = count >= 0 && (long)start + count <= text.Length ? start + count : text.Length;
        if (end < start)
            end = start;

        return text.Substring(start, end - start);
    }

    private static Avm1Value[] CreateRegisters()
    {
        var registers = new Avm1Value[256];
        Array.Fill(registers, Avm1Value.Undefined);
        return registers;
    }

    private bool ToBoolString(string text)
    {
        if (_version >= 7)
            return text.Length != 0;

        var number = StringToNumber(text);
        return !double.IsNaN(number) && number != 0.0;
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
