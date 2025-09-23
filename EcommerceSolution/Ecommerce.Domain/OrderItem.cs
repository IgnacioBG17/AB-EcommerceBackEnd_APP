using Ecommerce.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Domain
{
    public class OrderItem : BaseDomainModel
    {
        public Product? Product { get; set; }
        public int ProductId { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public Order? Order { get; set; }
        public int OrderId { get; set; }
        public int ProductoItemId { get; set; }
        public string? ProductNombre { get; set; }
        public string ImagenUrl { get; set; }
    }
}
