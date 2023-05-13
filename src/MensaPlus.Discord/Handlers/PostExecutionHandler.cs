using System.Threading;
using System.Threading.Tasks;

using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace MensaPlus.Discord.Handlers;

public sealed class PostExecutionHandler : IPostExecutionEvent
{
    private readonly FeedbackService _service;

    public PostExecutionHandler(FeedbackService service)
    {
        _service = service;
    }

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct = default)
    {
        if (commandResult.IsSuccess)
        {
            return (Result)commandResult;
        }

        IResultError error = commandResult.Error;

        return (Result)await _service.SendContextualErrorAsync($"""
            MensaPlus encountered an error while attempting to send a response:
            
            ``{error.GetType().Name}``
            > *{error.Message}*
            """);
    }
}
