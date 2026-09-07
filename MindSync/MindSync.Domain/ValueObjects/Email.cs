using MindSync.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace MindSync.Domain.ValueObjects;

public class Email
{
    public string Value { get; private set; } = null!;

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email() { }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("O endereço de e-mail não pode ser vazio.");

        value = value.Trim();

        if (!EmailRegex.IsMatch(value))
            throw new DomainException("O formato do e-mail fornecido é inválido.");

        Value = value;
    }

    public override string ToString() => Value;
}
