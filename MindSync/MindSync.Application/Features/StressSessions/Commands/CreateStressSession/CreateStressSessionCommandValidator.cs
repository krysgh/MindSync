using FluentValidation;

namespace MindSync.Application.Features.StressSessions.Commands.CreateStressSession;

public class CreateStressSessionCommandValidator : AbstractValidator<CreateStressSessionCommandRequest>
{
    public CreateStressSessionCommandValidator()
    {
        RuleFor(x => x.StressLevelBefore)
            .IsInEnum().WithMessage("O nível de estresse inicial informado é inválido.");

        RuleFor(x => x.TargetFrequency)
            .NotEmpty().WithMessage("A frequência alvo utilizada é obrigatória.");
    }
}
