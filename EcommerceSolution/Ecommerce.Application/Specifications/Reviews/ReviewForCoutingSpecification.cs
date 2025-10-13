using Ecommerce.Domain;

namespace Ecommerce.Application.Specifications.Reviews
{
    public class ReviewForCoutingSpecification : BaseSpecification<Review>
    {
        public ReviewForCoutingSpecification(ReviewSpecificationParams reviewParams)
            : base(
                  x => 
                  (!reviewParams.ProductId.HasValue || x.ProductId == reviewParams.ProductId)
            )
        {

        }
    }
}
