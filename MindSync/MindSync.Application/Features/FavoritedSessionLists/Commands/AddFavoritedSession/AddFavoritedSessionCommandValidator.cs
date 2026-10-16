using FluentValidation;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.AddFavoritedSession;

public class AddSessionToListCommandValidator : AbstractValidator<AddFavoritedSessionCommandRequest>
{
    public AddSessionToListCommandValidator()
    {
        RuleFor(x => x.FavoritedListId)
            .NotEmpty()
            .WithMessage("O ID da lista de favoritos é obrigatório.");

        RuleFor(x => x.StressSessionId)
            .NotEmpty()
            .WithMessage("O ID da sessão de estresse é obrigatório.");

        RuleFor(x => x.CustomName)
            .MaximumLength(100)
            .WithMessage("O nome customizado da sessão deve ter no máximo 100 caracteres.");
    }
}