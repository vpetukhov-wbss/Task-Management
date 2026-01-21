using FluentValidation;

namespace Modules.Tasks.Features.Boards.UpdateBoard;

public sealed class UpdateBoardValidator : AbstractValidator<UpdateBoardRequest>
{
	public UpdateBoardValidator()
	{
		RuleFor(x => x.BoardId)
			.NotEmpty().WithMessage("Board ID is required");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Title is required")
			.MaximumLength(200).WithMessage("Title cannot exceed 200 characters");
	}
}

