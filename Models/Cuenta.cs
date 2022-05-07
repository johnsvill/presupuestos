using System.ComponentModel.DataAnnotations;
using Presupuestos.Validaciones;

namespace Presupuestos.Models
{
    public class Cuenta
    {
        public int CuentaId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50)]
        [PrimeraLetraMayus]
        public string Nombre { get; set; }

        [Display(Name = "Tipo de cuenta")]
        public int TipoCuentaId { get; set; }

        public decimal Balance { get; set; }

        [StringLength(maximumLength: 1000)]
        public string Descripcion { get; set; }

        public string TipoCuenta { get; set; }  
    }
}
