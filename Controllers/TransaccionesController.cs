using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presupuestos.Models;
using Presupuestos.Servicios.Categorias;
using Presupuestos.Servicios.Cuentas;
using Presupuestos.Servicios.Transacciones;
using Presupuestos.Servicios.Usuarios;
using Presupuestos.ViewModels;

namespace Presupuestos.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly ITransacciones _transacciones;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IRepositorioCuentas _repositorioCuentas;
        private readonly ICategorias _categorias;

        public TransaccionesController(ITransacciones transacciones,
            IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas,
            ICategorias categorias)
        {
            this._transacciones = transacciones;
            this._servicioUsuarios = servicioUsuarios;
            this._repositorioCuentas = repositorioCuentas;
            this._categorias = categorias;
        }

        public async Task<ActionResult> Crear()
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var modelo = new TransaccionViewModel();

            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

            return await Task.Run(() => View(modelo));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await this._repositorioCuentas.BuscarCuenta(usuarioId);

            return cuentas.Select(x => new SelectListItem
                                           (x.Nombre, x.CuentaId.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId,
            TipoOperacion tipoOperacion)
        {
            var categorias = await this._categorias.Obtener(usuarioId, tipoOperacion);

            return categorias.Select(x => new SelectListItem(x.Nombre, x.CategoriaId.ToString()));
        }

        public async Task<ActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);

            return Ok(categorias);
        }
    }
}
