using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace ShockwaveFlash.SourceGenerators;

file sealed record DisposableAction(Action Action) : IDisposable
{
    public void Dispose()
    {
        Action();
    }
}

[DebuggerDisplay("{ToString(),nq}")]
internal sealed class IndentedStringBuilder
{
    private readonly StringBuilder _builder;

    private int _indentation;

    public IndentedStringBuilder()
    {
        _builder = new StringBuilder();
    }

    public IndentedStringBuilder Indent()
    {
        _indentation++;
        return this;
    }

    public IndentedStringBuilder Unindent()
    {
        if (_indentation > 0)
            _indentation--;
        return this;
    }

    public IndentedStringBuilder Append(char value)
    {
        _builder.Append(value);
        return this;
    }

    public IndentedStringBuilder Append(string value)
    {
        _builder.Append(value);
        return this;
    }

    public IndentedStringBuilder Append([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string value, params object[] args)
    {
        _builder.AppendFormat(CultureInfo.InvariantCulture, value, args);
        return this;
    }

    public IndentedStringBuilder AppendIndented(char value)
    {
        _builder
            .Append(new string('\t', _indentation))
            .Append(value);
        return this;
    }

    public IndentedStringBuilder AppendIndented(string value)
    {
        _builder
            .Append(new string('\t', _indentation))
            .Append(value);
        return this;
    }

    public IndentedStringBuilder AppendIndented([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string value, params object[] args)
    {
        _builder
            .Append(new string('\t', _indentation))
            .AppendFormat(CultureInfo.InvariantCulture, value, args);
        return this;
    }

    public IndentedStringBuilder AppendIndentedLine(char value)
    {
        _builder
            .Append(new string('\t', _indentation))
            .Append(value)
            .AppendLine();
        return this;
    }

    public IndentedStringBuilder AppendIndentedLine(string value)
    {
        _builder
            .Append(new string('\t', _indentation))
            .AppendLine(value);
        return this;
    }

    public IndentedStringBuilder AppendIndentedLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string value, params object[] args)
    {
        _builder
            .Append(new string('\t', _indentation))
            .AppendFormat(CultureInfo.InvariantCulture, value, args)
            .AppendLine();
        return this;
    }

    public IndentedStringBuilder AppendLine()
    {
        _builder.AppendLine();
        return this;
    }

    public IndentedStringBuilder AppendLine(char value)
    {
        _builder
            .Append(value)
            .AppendLine();
        return this;
    }

    public IndentedStringBuilder AppendLine(string value)
    {
        _builder.AppendLine(value);
        return this;
    }

    public IndentedStringBuilder AppendLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string value, params object[] args)
    {
        _builder
            .AppendFormat(CultureInfo.InvariantCulture, value, args)
            .AppendLine();
        return this;
    }

    public IndentedStringBuilder Clear()
    {
        _builder.Clear();
        _indentation = 0;
        return this;
    }

    public IDisposable CreateScope()
    {
        _builder
            .Append(new string('\t', _indentation))
            .AppendLine("{");

        _indentation++;

        return new DisposableAction(() =>
        {
            if (_indentation > 0)
                _indentation--;

            _builder
                .Append(new string('\t', _indentation))
                .AppendLine("}");
        });
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}
