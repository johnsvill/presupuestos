using Microsoft.AspNetCore.Mvc.Rendering;
using Presupuestos.Models;

namespace Presupuestos.ViewModels
{
    public class CuentaViewModel : Cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}
