namespace Ecommerce.Application.Features.Dashboard.Vms
{
    public class DashboardSummaryVm
    {
        public decimal TotalVentas { get; set; }
        public int TotalOrdenes { get; set; }
        public int OrdenesPendientes { get; set; }
        public int OrdenesCompletadas { get; set; }
        public int OrdenesEnviadas { get; set; }

        public int TotalProductos { get; set; }
        public int TotalUsuarios { get; set; }

        public decimal IngresosMesActual { get; set; }
        public decimal IngresosMesAnterior { get; set; }
        public decimal VariacionPorcentual { get; set; }
    }
}
