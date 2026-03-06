namespace Ecommerce.Application.Features.Auths.Users.Vms
{
    public class UserVm
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? Rol { get; set; }
        public bool IsActive { get; set; }
    }
}
