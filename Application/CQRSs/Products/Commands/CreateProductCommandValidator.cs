using FluentValidation;
using System.Reflection.Metadata.Ecma335;

namespace Application.CQRSs.Products.Commands
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        //CONSTRUCTOR
        public CreateProductCommandValidator()
        {
            RuleFor(product => product.Name)
                .NotEmpty()
                .NotNull()
                .WithMessage("{PropertyName} is required.")
                .Length(2, 15)
                .WithMessage("{PropertyName} must between 2 & 15 characters.");

            RuleFor(product => product.Remarks)
                .NotEmpty()
                .NotNull()
                .WithMessage("{PropertyName} is required.");

            RuleFor(product => product.Rate)
                .NotEmpty()
                .NotNull()
                .WithMessage("{PropertyName} is required.")
                .GreaterThan(0)
                .LessThan(500)
                .WithMessage("Rate field must contain only numeric value.");
            //.Must(BeNumricValueOnly);
                
        }

        private bool BeNumricValueOnly(int rate)
        {
            if (rate < 0)
            {
                return false;
            }
            return true;
        }
    }
}
