using System.Globalization;
using System.Net.Mail;

namespace Tasker.Auth.Domain.ValueObjects;

public readonly record struct EmailAddress
{
    public string Value { get; }
    
    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Email is required.", nameof(raw));
        }
        
        raw = raw.Trim();
        
        var atIndex = raw.IndexOf('@');
        if (atIndex <= 0 || atIndex == raw.Length - 1)
        {
            throw new ArgumentException($"Invalid email: '{raw}'.", nameof(raw));
        }

        var address = raw[..atIndex];
        var domainRaw = raw[(atIndex + 1)..];

        var idn = new IdnMapping();
        string domainAscii;

        try
        {
            domainAscii = idn.GetAscii(domainRaw).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            throw new ArgumentException($"Invalid email domain: '{domainRaw}'.", nameof(raw));
        }

        var normalized = $"{address}@{domainAscii}";
        try
        {
            _ = new MailAddress(normalized);
        }
        catch
        {
            throw new ArgumentException($"Invalid email: '{raw}'.", nameof(raw));
        }

        return new EmailAddress(normalized);
    }

    public static bool TryCreate(string? raw, out EmailAddress email)
    {
        try
        {
            email = EmailAddress.Create(raw ?? "");
            return true;
        }
        catch (Exception)
        {
            email = default;
            return false;
        }
    }
    
    public override string ToString() => Value;
    
    public static explicit operator string(EmailAddress email) => email.Value;
}