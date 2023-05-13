using System.Linq;

namespace OpenMensa.Extensions;

internal static class UriHelper
{
    public static string Build(params object[] segments)
    {
        return "/" + string.Join("/", segments);
    }

    public static string BuildParams(params (string ParameterName, object Argument)[] parameters)
    {
        return "?" + string.Join("&", parameters.Select(p => $"{p.ParameterName}={p.Argument}"));
    }
}
