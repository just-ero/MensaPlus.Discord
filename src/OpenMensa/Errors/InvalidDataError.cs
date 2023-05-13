using Remora.Results;

namespace OpenMensa.Errors;

public record InvalidDataError(string Message = "The received data was in an unexpected format.")
    : ResultError(Message);
