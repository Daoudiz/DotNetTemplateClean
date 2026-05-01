using System.Security.Cryptography;

namespace DotNetTemplateClean.Application;

public interface ITemporaryPasswordGenerator
{
    string Generate(int length = 16);
}

public class TemporaryPasswordGenerator : ITemporaryPasswordGenerator
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@$%*-_+?";

    public string Generate(int length = 16)
    {
        if (length < 12)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Temporary password length must be at least 12.");
        }

        Span<char> buffer = stackalloc char[length];

        // Ensure complexity requirements are always satisfied.
        buffer[0] = GetRandomChar(Upper);
        buffer[1] = GetRandomChar(Lower);
        buffer[2] = GetRandomChar(Digits);
        buffer[3] = GetRandomChar(Symbols);

        var all = string.Concat(Upper, Lower, Digits, Symbols);
        for (var i = 4; i < length; i++)
        {
            buffer[i] = GetRandomChar(all);
        }

        Shuffle(buffer);
        return new string(buffer);
    }

    private static char GetRandomChar(string source)
    {
        var index = RandomNumberGenerator.GetInt32(source.Length);
        return source[index];
    }

    private static void Shuffle(Span<char> buffer)
    {
        for (var i = buffer.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
        }
    }
}
