using FluentValidation;

namespace Modules.Users.Features.Roles.UpdateRole;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(256).WithMessage("Role name cannot exceed 256 characters");
    }
}

