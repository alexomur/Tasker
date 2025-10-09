using FluentValidation;

namespace Tasker.Application.Commands.Boards.CreateBoard;

public class CreateBoardValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title is too long");
    }
}