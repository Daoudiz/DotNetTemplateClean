using System.Diagnostics.CodeAnalysis;

namespace DotNetTemplateClean.Application;

[SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base() { }

    // Ajout du constructeur avec message    public ForbiddenAccessException(string message) : base(message) { }
}
