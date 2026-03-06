using Ecommerce.Application.Features.Auths.Users.Vms;
using Ecommerce.Application.Features.Shared.Queries;
using Ecommerce.Application.Persistence;
using Ecommerce.Application.Specifications.Users;
using Ecommerce.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Application.Features.Auths.Users.Queries.PaginationUsers
{
    public class PaginationUsersQueryHandler : IRequestHandler<PaginationUsersQuery, PaginationVm<UserVm>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Usuario> _userManager;

        public PaginationUsersQueryHandler(IUnitOfWork unitOfWork,
                                           UserManager<Usuario> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<PaginationVm<UserVm>> Handle(
                                                        PaginationUsersQuery request,
                                                        CancellationToken cancellationToken)
        {
            var userSpecificationParams = new UserSpecificationParams
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Search = request.Search,
                Sort = request.Sort
            };

            var spec = new UserSpecification(userSpecificationParams);
            var users = await _unitOfWork.Repository<Usuario>()
                .GetAllWithSpec(spec);

            var specCount = new UserForCoutingSpecification(userSpecificationParams);
            var totalUsers = await _unitOfWork.Repository<Usuario>()
                .CountAsync(specCount);

            var usersVm = new List<UserVm>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                usersVm.Add(new UserVm
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Telefono = user.PhoneNumber,
                    Rol = roles.FirstOrDefault()!, 
                    IsActive = user.IsActive
                });
            }

            return new PaginationVm<UserVm>
            {
                Count = totalUsers,
                Data = usersVm,
                PageCount = (int)Math.Ceiling(totalUsers / (double)request.PageSize),
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                ResultByPage = usersVm.Count
            };
        }

    }
}
