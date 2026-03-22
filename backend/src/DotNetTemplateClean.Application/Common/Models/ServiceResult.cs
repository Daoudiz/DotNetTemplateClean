namespace DotNetTemplateClean.Application;

/// <summary>
/// Classe générique pour encapsuler les résultats des services
/// </summary>
/// <typeparam name="T">Le type de donnée retournée en cas de succès</typeparam>
public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public int StatusCode { get; }

    // constructor made internal so non-generic factory can create instances
    internal ServiceResult(bool isSuccess, T? value, string? errorMessage, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    // Static factory members removed to satisfy CA1000
}

/// <summary>
/// Version non-générique pour les actions ne retournant pas de données
/// </summary>
public static class ServiceResult 
{

    // Succès sans données (pour les suppressions par exemple)
    public static ServiceResult<object?> Success(int statusCode = 200)
        => new(true, null, null, statusCode);

    // Échec générique
    public static ServiceResult<object?> Failure(string errorMessage, int statusCode = 400)
        => new(false, null, errorMessage, statusCode);

    // Generic factory methods moved here to avoid static members on the generic type
    public static ServiceResult<T> Success<T>(T value, int statusCode = 200)
        => new(true, value, null, statusCode);

    public static ServiceResult<T> Failure<T>(string errorMessage, int statusCode = 400)
        => new(false, default, errorMessage, statusCode);
}


