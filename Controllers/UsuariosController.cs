using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Models;

namespace Presupuestos.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public UsuariosController(UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
        }

       [HttpGet]     
       public async Task<ActionResult> Registro()
       {            
            return await Task.Run(() => View());       
       }

        [HttpPost, ActionName("Registro")]
        public async Task<ActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return await Task.Run(() => View(modelo));
            }

            var usuario = new Usuario()
            {
                Email = modelo.Email
            };

            var resultado = await this._userManager.CreateAsync(usuario, password: modelo.Password);

            if (resultado.Succeeded)
            {
                await this._signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("TransaccionesPorUsuario", "Transacciones");
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return await Task.Run(() => View(modelo));
            }            
        }
    }
}
