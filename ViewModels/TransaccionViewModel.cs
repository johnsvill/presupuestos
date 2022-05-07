using Microsoft.AspNetCore.Mvc.Rendering;
using Presupuestos.Models;
using System.ComponentModel.DataAnnotations;

namespace Presupuestos.ViewModels
{
    public class TransaccionViewModel : Transaccion
    {
        public IEnumerable<SelectListItem> Cuentas { get; set; }

        public IEnumerable<SelectListItem> Categorias { get; set; }

        [Display(Name = "Tipo operación")]
        public TipoOperacion TipoOperacionId { get; set; } //= TipoOperacion.Ingreso;
    }
}
