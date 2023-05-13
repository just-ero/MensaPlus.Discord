using System;
using System.Threading;
using System.Threading.Tasks;

using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Results;

namespace MensaPlus.Discord.Parsers;

public sealed class DateOnlyParser : AbstractTypeParser<DateOnly>
{
    public override ValueTask<Result<DateOnly>> TryParseAsync(string token, CancellationToken ct = default)
    {
        if (DateOnly.TryParse(token, out DateOnly result))
        {
            return new(result);
        }
        else
        {
            return new(new ParsingError<DateOnly>(token));
        }
    }
}
