using FluentValidation;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.RemoveFavoritedSession;

public class RemoveFavoritedSessionCommandValidator : AbstractValidator<RemoveFavoritedSessionCommandRequest>
{
    public RemoveFavoritedSessionCommandValidator()
    {
        RuleFor(x => x.FavoritedSessionId)
            .NotEmpty()
            .WithMessage("O ID da sessão favoritada é obrigatório.");
    }
}