using FluentValidation;

namespace Ecommerce.Application.Features.Addresses.Commands.CreateAddress
{
    public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressCommandValidator()
        {
            RuleFor(p => p.Direccion)
                .NotEmpty().WithMessage("La direccion no puede ser nula");

            RuleFor(p => p.Ciudad)
               .NotEmpty().WithMessage("La Ciudad no puede ser nula");

            RuleFor(p => p.Departamento)
               .NotEmpty().WithMessage("El Departamento no puede ser nulo");

            RuleFor(p => p.CodigoPostal)
               .NotEmpty().WithMessage("El CodigoPostal no puede ser nulo");

            RuleFor(p => p.Pais)
              .NotEmpty().WithMessage("El Pais no puede ser nulo");
        }
    }
}
