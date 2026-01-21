using FluentValidation;

namespace Modules.Tasks.Features.Boards.CreateBoard;

public sealed class CreateBoardValidator : AbstractValidator<CreateBoardRequest>
{
	public CreateBoardValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Title is required")
			.MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

		RuleFor(x => x.UserId)
			.NotEmpty().WithMessage("User ID is required");
	}
}

