using FluentValidation;

namespace MindSync.Application.Features.FavoritedSessionLists.Queries.GetFavoritedListstByUserId;

public class GetFavoritedListsByUserIdQueryValidator : AbstractValidator<GetFavoritedListsByUserIdQueryRequest>
{
    public GetFavoritedListsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("O ID do usuário é obrigatório.");
    }
}