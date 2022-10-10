using System.ComponentModel.DataAnnotations;

namespace Presupuestos.Models
{
    public class Transaccion
    {
        public int IdTransac { get; set; }

        public int UsuarioId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FechaTransac { get; set; } = DateTime.Parse(DateTime.Now.ToString("g"));

        public decimal Monto { get; set; }

        [Display(Name = "Categoría")]
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoria")]
        public int CategoriaId { get; set; }

        [StringLength(maximumLength: 1000, ErrorMessage = "La nota no puede pasar de {1} caracteres")]
        public string Nota { get; set; }

        [Display(Name = "Cuenta")]
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
        public int CuentaId { get; set; }

        public string Categoria { get; set; }

        public string Cuenta { get; set; }

        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Gasto;
    }
}
