using FluentValidation;

namespace Restaurants.Application.Dishes.Commands.CreateDish
{
    public class CreateDishCommandValidator : AbstractValidator<CreateDishCommand>
    {
        public CreateDishCommandValidator()
        {
            RuleFor(dish => dish.Description).NotNull()
                .Length(3, 32);

            RuleFor(dish => dish.Name).NotNull()
                .Length(3, 16);

            RuleFor(dish => dish.Price).NotNull()
                .GreaterThanOrEqualTo((decimal) 0.01);

            RuleFor(dish => dish.KiloCalories).NotNull()
                .GreaterThanOrEqualTo(0);
        }
    }
}
