using FluentValidation;
using Modules.Tasks.Domain.Tasks;

namespace Modules.Tasks.Features.Tasks.UpdateTask;

public sealed class UpdateTaskValidator : AbstractValidator<UpdateTaskRequest>
{
	public UpdateTaskValidator()
	{
		RuleFor(x => x.TaskId)
			.NotEmpty().WithMessage("Task ID is required");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Title is required")
			.MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
			.When(x => !string.IsNullOrEmpty(x.Description));

		RuleFor(x => x.Priority)
			.Must(p => Enum.IsDefined(typeof(TaskPriority), p))
			.WithMessage("Invalid priority value. Valid values are: 0 (Low), 1 (Medium), 2 (High)");
	}
}

