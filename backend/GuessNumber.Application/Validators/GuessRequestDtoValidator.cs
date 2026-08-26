using FluentValidation;
using GuessNumber.Application.DTOs;

namespace GuessNumber.Application.Validators;

public class GuessRequestDtoValidator : AbstractValidator<GuessRequestDto>
{
    public GuessRequestDtoValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.Guess)
            .InclusiveBetween(1, 43)
            .WithMessage("Your guess must be between 1 and 43.");
    }
}