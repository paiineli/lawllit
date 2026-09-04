namespace Lawllit.Model.Common;

// Corpo padrão de erro de negócio da API. O Site converte de volta para Result
// e só então traduz a ErrorKey, mantendo a API livre de cultura.
public sealed class ApiErrorMOD
{
    public string ErrorKey { get; set; } = string.Empty;
}
