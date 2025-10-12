using Ecommerce.Domain;

namespace Ecommerce.Application.Specifications.Users
{
    public class UserForCoutingSpecification : BaseSpecification<Usuario>
    {
        public UserForCoutingSpecification(UserSpecificationParams userParams)
            : base(
            x =>
            (
                string.IsNullOrEmpty(userParams.Search) || x.Nombre!.Contains(userParams.Search)
            || x.Apellido!.Contains(userParams.Search) || x.Email!.Contains(userParams.Search)
            )
        )
        {
            
        }
    }
}
