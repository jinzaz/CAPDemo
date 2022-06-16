using FluentValidation;

namespace CAPDemo.Application.Validations;

public class IdentifiedCommandValidator : AbstractValidator<IdentifiedCommand<CreateUserCommand, bool>>
{
    public IdentifiedCommandValidator(ILogger<IdentifiedCommandValidator> logger)
    {
        RuleFor(command => command.Id).NotEmpty();

        logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
    }
}
