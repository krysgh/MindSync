using FluentValidation;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedSession;

public class UpdateFavoritedSessionCommandValidator : AbstractValidator<UpdateFavoritedSessionCommandRequest>
{
    public UpdateFavoritedSessionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O ID da sessão favoritada é obrigatório.");

        RuleFor(x => x.CustomName)
            .NotEmpty()
            .WithMessage("O nome customizado não pode ser vazio.")
            .MaximumLength(100)
            .WithMessage("O nome customizado deve ter no máximo 100 caracteres.");
    }
}