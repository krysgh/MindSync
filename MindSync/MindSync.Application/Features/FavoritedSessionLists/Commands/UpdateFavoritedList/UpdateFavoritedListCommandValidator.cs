using FluentValidation;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.UpdateFavoritedList;

public class UpdateFavoritedListCommandValidator : AbstractValidator<UpdateFavoritedListCommandRequest>
{
    public UpdateFavoritedListCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O ID da lista é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O nome da lista não pode ser vazio.")
            .MaximumLength(100)
            .WithMessage("O nome da lista deve ter no máximo 100 caracteres.");
    }
}