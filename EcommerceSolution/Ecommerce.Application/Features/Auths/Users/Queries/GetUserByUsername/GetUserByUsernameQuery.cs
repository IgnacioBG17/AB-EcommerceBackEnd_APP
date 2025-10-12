using Ecommerce.Application.Features.Auths.Users.Vms;
using MediatR;

namespace Ecommerce.Application.Features.Auths.Users.Queries.GetUserByUsername
{
    public class GetUserByUsernameQuery : IRequest<AuthResponse>
    {
        public string? UserName { get; set; }
        public GetUserByUsernameQuery(string username)
        {
            UserName = username ?? throw new ArgumentNullException(nameof(username));
        }
    }
}
