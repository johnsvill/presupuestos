using Microsoft.AspNetCore.Mvc;
using Presupuestos.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace Presupuestos.Models
{
    public class TipoCuenta  /*IValidatableObject*/
    {
        public int TipoCuentaId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayus]
        [Remote(action: "VerificarExisteTipocuenta", controller:"TiposCuentas")]
        public string Nombre { get; set; }

        public int UsuarioId { get; set; }  

        public int Orden { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if(Nombre != null && Nombre.Length > 0)
        //    {
        //        var primeraLetra = Nombre[0].ToString();

        //        if(primeraLetra != primeraLetra.ToUpper())
        //        {
        //            yield return new ValidationResult("La primera letra debe ser mayúscula.",
        //                new[] { nameof(Nombre) });                    
        //        }            
        //    }
        //}
    }
}
