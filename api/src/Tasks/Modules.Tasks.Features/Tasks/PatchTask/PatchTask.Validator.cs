using FluentValidation;
using Modules.Tasks.Domain.Tasks;

namespace Modules.Tasks.Features.Tasks.PatchTask;

public sealed class PatchTaskValidator : AbstractValidator<PatchTaskRequest>
{
	public PatchTaskValidator()
	{
		RuleFor(x => x.TaskId)
			.NotEmpty().WithMessage("Task ID is required");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Title cannot be empty")
			.MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
			.When(x => x.Title != null);

		RuleFor(x => x.Description)
			.MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
			.When(x => x.Description != null);

		RuleFor(x => x.Priority)
			.Must(p => Enum.IsDefined(typeof(TaskPriority), p!.Value))
			.WithMessage("Invalid priority value. Valid values are: 0 (Low), 1 (Medium), 2 (High)")
			.When(x => x.Priority.HasValue);

		RuleFor(x => x.Status)
			.Must(s => Enum.IsDefined(typeof(Domain.Tasks.TaskStatus), s!.Value))
			.WithMessage("Invalid status value. Valid values are: 0 (Incomplete), 1 (Complete), 2 (InProgress), 3 (OnHold)")
			.When(x => x.Status.HasValue);
	}
}

