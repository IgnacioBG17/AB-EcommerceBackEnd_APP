namespace Ecommerce.Application.Features.Dashboard.Vms
{
    public class SalesByCountryVm
    {
        public string Pais { get; set; } = string.Empty;
        public decimal TotalVentas { get; set; }
        public int CantidadOrdenes { get; set; }
    }
}
