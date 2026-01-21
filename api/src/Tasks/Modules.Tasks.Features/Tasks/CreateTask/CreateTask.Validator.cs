using FluentValidation;
using Modules.Tasks.Domain.Tasks;

namespace Modules.Tasks.Features.Tasks.CreateTask;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
	public CreateTaskValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Title is required")
			.MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
			.When(x => !string.IsNullOrEmpty(x.Description));

		RuleFor(x => x.Priority)
			.Must(p => Enum.IsDefined(typeof(TaskPriority), p))
			.WithMessage("Invalid priority value. Valid values are: 0 (Low), 1 (Medium), 2 (High)");

		RuleFor(x => x.UserId)
			.NotEmpty().WithMessage("User ID is required")
			.MaximumLength(450).WithMessage("User ID cannot exceed 450 characters");

		RuleFor(x => x.BoardId)
			.NotEmpty().WithMessage("Board ID is required");
	}
}

