using FluentValidation;

namespace Restaurants.Application.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommandValidator : AbstractValidator<UpdateRestaurantCommand>
{
    public UpdateRestaurantCommandValidator()
    {
        RuleFor(dto => dto.Name).NotNull()
            .Length(3, 32);

        RuleFor(dto => dto.Description).NotNull()
            .Length(3, 32);

        RuleFor(dto => dto.HasDelivery).NotNull();
    }
}
