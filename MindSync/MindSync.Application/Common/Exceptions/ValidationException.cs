namespace MindSync.Application.Common.Exceptions;

public class ValidationException(IDictionary<string, string[]> errors) : Exception("Ocorreram um ou mais erros de validação.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
