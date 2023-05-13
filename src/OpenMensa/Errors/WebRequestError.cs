using System.Net;

using Remora.Results;

namespace OpenMensa.Errors;

public record WebRequestError(HttpStatusCode StatusCode, string Message = "A web request has failed.")
    : ResultError($"{Message} ({(int)StatusCode} {StatusCode})");
