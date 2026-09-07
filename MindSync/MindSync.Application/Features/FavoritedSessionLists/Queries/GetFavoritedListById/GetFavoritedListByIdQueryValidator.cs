using FluentValidation;

namespace MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListById;

public class GetFavoritedListByIdQueryValidator : AbstractValidator<GetFavoritedListByIdQueryRequest>
{
    public GetFavoritedListByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O ID da lista é obrigatório.");
    }
}