using System.ComponentModel.DataAnnotations;

namespace Pedidos360Proyecto.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cedula es obligatoria.")]
        [StringLength(20, ErrorMessage = "La cedula no puede superar los 20 caracteres.")]
        [Display(Name = "Cedula")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato valido.")]
        [StringLength(150)]
        [Display(Name = "Correo")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El telefono es obligatorio.")]
        [Phone(ErrorMessage = "El telefono no tiene un formato valido.")]
        [StringLength(20)]
        [Display(Name = "Telefono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La direccion es obligatoria.")]
        [StringLength(250)]
        [Display(Name = "Direccion")]
        public string Direccion { get; set; } = string.Empty;

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
