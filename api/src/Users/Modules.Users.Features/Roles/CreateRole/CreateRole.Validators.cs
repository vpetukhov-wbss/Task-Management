using FluentValidation;

namespace Modules.Users.Features.Roles.CreateRole;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(256).WithMessage("Role name cannot exceed 256 characters");
    }
}

