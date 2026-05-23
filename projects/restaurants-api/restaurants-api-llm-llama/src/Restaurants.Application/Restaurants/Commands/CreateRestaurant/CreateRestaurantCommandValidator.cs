using FluentValidation;
using Restaurants.Application.Restaurants.Dtos;

namespace Restaurants.Application.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandValidator : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantCommandValidator()
    {
        RuleFor(dto => dto.Name).NotNull()
            .Length(3, 32);

        RuleFor(dto => dto.Category).NotNull()
            .Length(3, 16);

        RuleFor(dto => dto.Description).NotNull()
            .Length(3, 32);

        RuleFor(dto => dto.HasDelivery).NotNull();

        RuleFor(dto => dto.City).NotNull()
            .Length(3, 16);

        RuleFor(dto => dto.Street).NotNull()
            .Length(3, 32);

        RuleFor(dto => dto.ContactNumber).NotNull()
            .Length(8, 13)
            .Matches("^[0-9]*$");

        RuleFor(dto => dto.PostalCode).NotNull()
            .Length(3, 10)
            .Matches("^[0-9]*$");

        RuleFor(dto => dto.ContactEmail).NotNull()
            .MinimumLength(5)
            .EmailAddress()
            .WithMessage("Please provide a valid email address");
    }
}
