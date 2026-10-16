using FluentValidation;

namespace MindSync.Application.Features.SessionAudioMixes.Commands.GetOrCreateSessionAudioMix;

public class GetOrCreateSessionAudioMixCommandValidator : AbstractValidator<GetOrCreateSessionAudioMixCommandRequest>
{
    public GetOrCreateSessionAudioMixCommandValidator()
    {
        RuleFor(x => x.StressSessionId)
            .NotEmpty()
            .WithMessage("O ID da sessão de estresse é obrigatório.");
    }
}