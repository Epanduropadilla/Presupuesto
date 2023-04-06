using Microsoft.AspNetCore.Mvc;
using Presupuesto.Validaciones;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Presupuesto.Models
{
	public class TiposCuentas
	{
        public int Id { get; set; }

		[Required(ErrorMessage ="El campo Nombre es requerido")]
		[Display(Name ="Nombre de Tipo de Cuenta")]
		[PrimeraLetraMayuscula]
		[Remote(action: "VerificarExisteTipoCuenta", controller:"TiposCuentas")]
							// En controller: se llama al controlador, pero sin colocar el
							// Controller de TiposCuentasController
		public string? Nombre { get; set; }
		public int Orden { get; set; }
		public int UsuarioId { get; set; }
		
	}
}

