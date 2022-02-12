using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;
using Presupuestos.Servicios;
using Presupuestos.Servicios.Usuarios;

namespace Presupuestos.Controllers
{
    public class TiposCuentasController : Controller
    {
        //private readonly string connectionString;
        private readonly IRepositorioTiposCuentas _repositorioTiposCuentas;

        private readonly IServicioUsuarios _servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
            IServicioUsuarios servicioUsuarios)
        {
            this._repositorioTiposCuentas = repositorioTiposCuentas;
            this._servicioUsuarios = servicioUsuarios;
        }

        public IActionResult Crear()
        {
            //using(var connection = new SqlConnection(connectionString))
            //{                
            //    var query = connection.Query("SELECT 1").FirstOrDefault();

            //    ViewBag.Query = query;
            //}

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta =
                await this._repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre),
                    $"El nombre {tipoCuenta.Nombre} ya existe.");

                return View(tipoCuenta);
            }

            await this._repositorioTiposCuentas.CrearTipoCuenta(tipoCuenta);

            return RedirectToAction("ListaTiposCuentas");
        }

        [HttpGet]
        public async Task<ActionResult> EditarTipoCuenta(int TipoCuentaId)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var tipoCuenta = await 
                this._repositorioTiposCuentas.ObtenerPorId(TipoCuentaId, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");    
            }

            return View(tipoCuenta);
        }

        [HttpPost, ActionName("EditarTipoCuenta")]
        public async Task<ActionResult> EditarPostTipoCuenta(TipoCuenta tipoCuenta)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var tipoCuentaExiste = await
                   this._repositorioTiposCuentas
                        .ObtenerPorId(tipoCuenta.TipoCuentaId, usuarioId);

            if(tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home"); 
            }

            await this._repositorioTiposCuentas.UpdateTipoCuentas(tipoCuenta);

            return RedirectToAction("ListaTiposCuentas");
        }

        [HttpGet]
        public async Task<ActionResult> VerificarExisteTipocuenta(string nombre)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await this._repositorioTiposCuentas
                                                 .Existe(nombre, usuarioId);

            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe."); 
            }

            return Json(true);
        }

        [HttpGet]
        public async Task<ActionResult> ListaTiposCuentas()
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var tiposCuentas = await this._repositorioTiposCuentas  
                .ObtenerTiposCuentas(usuarioId);
           
            return View(tiposCuentas);
        }

        [HttpGet]
        public async Task<ActionResult> EliminarTipoCuenta(int TipoCuentaId)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var tipoCuenta = await this._repositorioTiposCuentas
                .ObtenerPorId(TipoCuentaId, usuarioId);

            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost, ActionName("EliminarTipoCuenta")]
        public async Task<ActionResult> EliminarPostTipoCuenta(int TipoCuentaId)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var tipoCuenta = await this._repositorioTiposCuentas
                .ObtenerPorId(TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

           await this._repositorioTiposCuentas
                .EliminarTipoCuenta(TipoCuentaId);

            return RedirectToAction("ListaTiposCuentas");
        }

        [HttpPost]
        public async Task<ActionResult> OrdenarTiposCuentas([FromBody] int[] ids)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var tiposCuentas = await this._repositorioTiposCuentas.ObtenerTiposCuentas(usuarioId);

            var idsTiposCuentas = tiposCuentas.Select(x => x.TipoCuentaId).ToList();

            //Para validar los ids del frontend con los de la DB
            var idsPertenecenUsuario = ids.Except(idsTiposCuentas).ToList();

            if(idsPertenecenUsuario.Count() > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) =>
                          new TipoCuenta() { TipoCuentaId = valor, Orden = indice + 1 }).AsEnumerable();

            await this._repositorioTiposCuentas.OrdenarTiposCuentas(tiposCuentasOrdenados);

            return Ok();
        }
    }
}
