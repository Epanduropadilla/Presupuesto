using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
//using System.Data;
using Presupuesto.Models;
using Presupuesto.Servicios;

namespace Presupuesto.Controllers
{
	public class TiposCuentasController: Controller
	{
		private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
		private readonly IServicioUsuarios servicioUsuario;

		public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
			IServicioUsuarios servicioUsuario)
		{
			this.repositorioTiposCuentas = repositorioTiposCuentas;
			this.servicioUsuario = servicioUsuario;
		}
		// Por convesion se denomina Index a la accion que devuelve un lista 
		public async Task<IActionResult> Index()
		{
			var UsuarioId = servicioUsuario.ObtenerUsuarioId();
			var tiposCuentas = await repositorioTiposCuentas.Obtener(UsuarioId);
			return View(tiposCuentas);
		}

		public IActionResult Crear()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Crear(TiposCuentas tiposCuentas)
		{
			if (!ModelState.IsValid) 
			{
				return View(tiposCuentas);
			}

			tiposCuentas.UsuarioId = servicioUsuario.ObtenerUsuarioId();
			var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(
									tiposCuentas.Nombre, tiposCuentas.UsuarioId	
																		);
			if (yaExisteTipoCuenta)
			{
				ModelState.AddModelError(nameof(tiposCuentas.Nombre),
							$"El nombre {tiposCuentas.Nombre} ya existe."
					                    );
				return View(tiposCuentas);
			}
			
			await repositorioTiposCuentas.Crear(tiposCuentas);

			//return View();
			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<ActionResult> Editar(int Id)
		{
			var UsuarioId = servicioUsuario.ObtenerUsuarioId();
			var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(Id, UsuarioId);
			if (tipoCuenta is null) 
			{
				// estavista se creara en un momento
				return RedirectToAction("NoEncontrado", "Home");
			}
			return View(tipoCuenta);
		}

		[HttpPost]
		public async Task<ActionResult> Editar(TiposCuentas tiposCuentas)
		{
			var UsuarioId = servicioUsuario.ObtenerUsuarioId();
			var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(
				tiposCuentas.Id, UsuarioId);

			if (tipoCuentaExiste is null)
			{			
				return RedirectToAction("NoEncontrado", "Home");
			}
			await repositorioTiposCuentas.Actualizar(tiposCuentas);
			return RedirectToAction("Index"); ;
		}

		[HttpGet]
		public async Task<IActionResult> VerificarExisteTipoCuenta(string Nombre)
		{
			var UsuarioId = servicioUsuario.ObtenerUsuarioId();
			var yaExisteTipoCuenta = await 
				repositorioTiposCuentas.Existe(Nombre, UsuarioId);

			if (yaExisteTipoCuenta)
			{							
				return Json($"El nombre {Nombre} ya existe.");
			}

			return Json(true);
		}

		[HttpGet]
		public async Task<IActionResult> Borrar(int Id)
		{
			var UsuarioId = servicioUsuario.ObtenerUsuarioId();
			var tiposCuentas = await repositorioTiposCuentas.ObtenerPorId(Id, UsuarioId);
			if (tiposCuentas is null)
			{ 
				return RedirectToAction("NoEncontrado","Home");
			}
			return View(tiposCuentas);
		}

		[HttpPost]
		public async Task<IActionResult> BorrarTipoCuenta(int Id)
		{
			var UsuarioId = servicioUsuario.ObtenerUsuarioId();
			var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(Id, UsuarioId);
			if (tipoCuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}
			await repositorioTiposCuentas.Borrar(Id);
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Ordenar([FromBody] int[] ids)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
			var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

			var idsTiposCuentasNoPertenecenAlusuario = ids.Except(idsTiposCuentas).ToList();

			if (idsTiposCuentasNoPertenecenAlusuario.Count > 0)
			{
				return Forbid(); // Prohibir
			}

			var tiposCuentasOrdenados = ids.Select((valor, indice) => 
							new TiposCuentas() { Id = valor, Orden = indice + 1}
							).AsEnumerable();

			await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);

			return Ok(); // 200
		}		

	}
}
