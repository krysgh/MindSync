using FluentValidation;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.CreateFavoritedList;

public class CreateFavoritedListCommandValidator : AbstractValidator<CreateFavoritedListCommandRequest>
{
    public CreateFavoritedListCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O nome da lista não pode ser vazio.")
            .MaximumLength(100)
            .WithMessage("O nome da lista deve ter no máximo 100 caracteres.");
    }
}