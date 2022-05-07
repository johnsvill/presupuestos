using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presupuestos.Servicios;
using Presupuestos.Servicios.Cuentas;
using Presupuestos.Servicios.Usuarios;
using Presupuestos.ViewModels;

namespace Presupuestos.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas _repositorioTiposCuentas;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IRepositorioCuentas _repositorioCuentas;
        private readonly IMapper _mapper;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
            IServicioUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas,
            IMapper mapper)
        {
            this._repositorioTiposCuentas = repositorioTiposCuentas;
            this._servicioUsuarios = servicioUsuarios;
            this._repositorioCuentas = repositorioCuentas;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task <ActionResult> CrearCuenta()
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();
            
            var modelo = new CuentaViewModel();

            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return await Task.Run(() => View(modelo));
        }

        [HttpPost, ActionName("CrearCuenta")]
        public async Task<ActionResult> CrearCuentaPost(CuentaViewModel cuenta)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var tiposCuentas = await this._repositorioTiposCuentas
                        .ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if(tiposCuentas is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

                return View(cuenta);
            }

            await this._repositorioCuentas.CrearCuenta(cuenta);

            return RedirectToAction("ListaTiposCuentas", "TiposCuentas");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await this._repositorioTiposCuentas.ObtenerTiposCuentas(usuarioId);

            return tiposCuentas.Select(x => new SelectListItem
                                              (x.Nombre, x.TipoCuentaId.ToString()));
        }

        public async Task<ActionResult> BalanceTiposCuentas()   
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var cuentasConTipoCuenta = await this._repositorioCuentas.BuscarCuenta(usuarioId);

            var modelo = cuentasConTipoCuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentasViewModel
                {
                    TipoCuenta = grupo.Key,//representa el valor con el que se hace el GroupBy
                    Cuentas = grupo.AsEnumerable()

                }).ToList();

            return await Task.Run(() => View(modelo));
        }

        [HttpGet]
        public async Task<ActionResult> EditarCuenta(int id)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var cuenta = await this._repositorioCuentas.ObtenerPorId(id, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
          
            var modelo = this._mapper.Map<CuentaViewModel>(cuenta);

            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return await Task.Run(() => View(modelo));
        }

        [HttpPost, ActionName("EditarCuenta")]
        public async Task<ActionResult> EditarCuentaPost(CuentaViewModel cuentaViewModel)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var cuenta = await this._repositorioCuentas
                .ObtenerPorId(cuentaViewModel.CuentaId, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await this._repositorioTiposCuentas
                .ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await this._repositorioCuentas.Actualizar(cuentaViewModel);

            return RedirectToAction("BalanceTiposCuentas", "Cuentas");
        }

        [HttpGet]
        public async Task<ActionResult> Borrar(int id)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var cuenta = await this._repositorioCuentas
                .ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return await Task.Run(() => View(cuenta));
        }

        [HttpPost, ActionName("Borrar")]
        public async Task<ActionResult> BorrarPostCuenta(int id)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var cuenta = await this._repositorioCuentas
                .ObtenerPorId(id, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
                
            await this._repositorioCuentas.Borrar(id);

            return RedirectToAction("BalanceTiposCuentas");
        }
    }
}
