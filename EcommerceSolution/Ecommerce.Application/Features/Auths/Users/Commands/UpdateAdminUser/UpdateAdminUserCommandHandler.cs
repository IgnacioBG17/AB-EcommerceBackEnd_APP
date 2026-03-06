using Ecommerce.Application.Contracts.Identity;
using Ecommerce.Application.Exceptions;
using Ecommerce.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Application.Features.Auths.Users.Commands.UpdateAdminUser
{
    public class UpdateAdminUserCommandHandler : IRequestHandler<UpdateAdminUserCommand, Usuario>
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthService _authService;

        public UpdateAdminUserCommandHandler(UserManager<Usuario> userManager, 
                                            RoleManager<IdentityRole> roleManager, 
                                            IAuthService authService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _authService = authService;
        }

        public async Task<Usuario> Handle(UpdateAdminUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscar el usuario
            var updateUsuario = await _userManager.FindByIdAsync(request.Id!);
            if (updateUsuario is null)
            {
                throw new BadRequestException("El usuario no existe");
            }

            // 2. Actualizar datos básicos
            updateUsuario.Nombre = request.Nombre;
            updateUsuario.Apellido = request.Apellido;
            updateUsuario.Telefono = request.Telefono;

            var resultado = await _userManager.UpdateAsync(updateUsuario);

            if (!resultado.Succeeded)
            {
                throw new Exception("No se pudo actualizar el usuario");
            }

            // 3. Manejo de Roles
            var role = await _roleManager.FindByNameAsync(request.Role!);
            if (role is null)
            {
                throw new BadRequestException("El Rol asignado no existe");
            }

            // Obtener los roles actuales que tiene el usuario en la DB
            var rolesActuales = await _userManager.GetRolesAsync(updateUsuario);

            if (rolesActuales.Count != 1 || rolesActuales[0] != request.Role)
            {
                // 3. Eliminamos TODOS los roles que tenga (limpiamos la mesa)
                if (rolesActuales.Any())
                {
                    await _userManager.RemoveFromRolesAsync(updateUsuario, rolesActuales);
                }

                // 4. Asignamos el rol único solicitado
                var resultadoRol = await _userManager.AddToRoleAsync(updateUsuario, request.Role!);

                if (!resultadoRol.Succeeded)
                {
                    throw new Exception("Error al asignar el rol único");
                }
            }

            return updateUsuario;
        }
    }
}
