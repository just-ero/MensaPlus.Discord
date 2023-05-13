using System.Threading;
using System.Threading.Tasks;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace MensaPlus.Discord.Handlers;

public sealed class PreparationErrorHandler : IPreparationErrorEvent
{
    private readonly FeedbackService _service;

    public PreparationErrorHandler(FeedbackService service)
    {
        _service = service;
    }

    public async Task<Result> PreparationFailed(IOperationContext context, IResult preparationResult, CancellationToken ct = default)
    {
        if (preparationResult.IsSuccess)
        {
            return (Result)preparationResult;
        }

        IResultError error = preparationResult.Error;

        return (Result)await _service.SendContextualErrorAsync($"""
            MensaPlus encountered an error while attempting to prepare a response:

            ``{error.GetType().Name}``
            > *{error.Message}*
            """,
            options: new(MessageFlags: MessageFlags.Ephemeral),
            ct: ct);
    }
}
