using Microsoft.AspNetCore.Mvc;
using Presupuestos.Models;
using Presupuestos.Servicios.Categorias;
using Presupuestos.Servicios.Usuarios;

namespace Presupuestos.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ICategorias _servicioCategorias;
        private readonly IServicioUsuarios _servicioUsuarios;

        public CategoriasController(ICategorias servicioCategorias,
            IServicioUsuarios servicioUsuarios)
        {
            this._servicioCategorias = servicioCategorias;
            this._servicioUsuarios = servicioUsuarios;
        }

        [HttpGet]
        public async Task<ActionResult> Crear()
        {
            return await Task.Run(() => View());
        }

        [HttpPost]
        public async Task<ActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return await Task.Run(() => View(categoria));
            }

            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            categoria.UsuarioId = usuarioId;

            await this._servicioCategorias.Crear(categoria);

            return RedirectToAction("CategoriaUsuario");
        }

        public async Task<ActionResult> CategoriaUsuario()
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var categorias = await this._servicioCategorias.Obtener(usuarioId);

            return await Task.Run(() => View(categorias));  
        }

        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var categoria = await this._servicioCategorias.ObtenerPorId(id, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return await Task.Run(() => View(categoria));
        }

        [HttpPost, ActionName("Editar")]
        public async Task<ActionResult> Editar(Categoria categoriaEditar)
        {
            if (!ModelState.IsValid)
            {
                return await Task.Run(() => View(categoriaEditar));
            }

            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var categoria = await this._servicioCategorias.ObtenerPorId(categoriaEditar.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoriaEditar.UsuarioId = usuarioId;

            await this._servicioCategorias.Actualizar(categoriaEditar);

            return RedirectToAction("CategoriaUsuario");
        }

        public async Task<ActionResult> Borrar(int id)
        {
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var categoria = await this._servicioCategorias.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return await Task.Run(() => View(categoria));   
        }
            
        [HttpPost, ActionName("Borrar")]
        public async Task<ActionResult> BorrarPost(int id )
        {          
            var usuarioId = this._servicioUsuarios.ObtenerUsuarioId();

            var categoria = await this._servicioCategorias.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
           
            await this._servicioCategorias.Borrar(id);

            return RedirectToAction("CategoriaUsuario");
        }
    }
}
