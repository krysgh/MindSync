using FluentValidation;

namespace MindSync.Application.Features.FavoritedSessionLists.Commands.DeleteFavoritedList;

public class DeleteFavoritedListCommandValidator : AbstractValidator<DeleteFavoritedListCommandRequest>
{
    public DeleteFavoritedListCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O ID da lista a ser removida é obrigatório.");
    }
}