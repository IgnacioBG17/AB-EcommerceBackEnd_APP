namespace Ecommerce.Application.Features.Dashboard.Vms
{
    public class SalesByCategoryVm
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal TotalVentas { get; set; }
        public int CantidadVendida { get; set; }
    }
}
