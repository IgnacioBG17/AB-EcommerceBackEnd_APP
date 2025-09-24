using Ecommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Domain
{
    public class Image : BaseDomainModel
    {
        [Column(TypeName = "NVARCHAR(4000)")]
        public string? Url { get; set; }
        public int ProductId { get; set; }
        public string? PublicCode { get; set; }
        public virtual Product? Product { get; set; }
    }
}
