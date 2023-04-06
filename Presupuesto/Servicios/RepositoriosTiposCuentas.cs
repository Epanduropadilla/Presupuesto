using Dapper;
using Microsoft.Data.SqlClient;
using Presupuesto.Models;

namespace Presupuesto.Servicios
{
	public interface IRepositorioTiposCuentas
	{
		Task Actualizar(TiposCuentas tipoCuenta);
		Task Borrar(int Id);
		Task Crear(TiposCuentas tipoCuenta);
		Task<bool> Existe(string Nombre, int UsuarioId);
		Task<IEnumerable<TiposCuentas>> Obtener(int UsuarioId);
		Task<TiposCuentas> ObtenerPorId(int Id, int UsuarioId);
		Task Ordenar(IEnumerable<TiposCuentas> tipoCuentasOrdenados);
	}
	public class RepositoriosTiposCuentas: IRepositorioTiposCuentas
	{
		private readonly string connectionString;
        public RepositoriosTiposCuentas(IConfiguration configuration)
        {
			connectionString = configuration.GetConnectionString("DefaultConnection");
        }
		// Metodo Crear un tipo cuenta en la BD
		public async Task Crear(TiposCuentas tipoCuenta) {		
			using var connection = new SqlConnection(connectionString);
			var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar",
				                new { usuarioId = tipoCuenta.UsuarioId,
								      nombre = tipoCuenta.Nombre},
								      commandType: System.Data.CommandType.StoredProcedure);

			tipoCuenta.Id = id;
		}

		public async Task<bool> Existe(string Nombre, int UsuarioId)
		{
			using var connection = new SqlConnection(connectionString);
			var existe = await connection.QueryFirstOrDefaultAsync<int>(
							@"SELECT 1
							FROM TiposCuentas
							WHERE Nombre = @Nombre and UsuarioId = @UsuarioId;",
							new { Nombre, UsuarioId }
																		);
			return existe == 1;
		}

		public async Task<IEnumerable<TiposCuentas>> Obtener(int UsuarioId)
		{ 
			using var connection = new SqlConnection( connectionString);
			return await connection.QueryAsync<TiposCuentas>(
				@"SELECT Id, Nombre, Orden
					FROM TiposCuentas
					WHERE UsuarioId = @UsuarioId", new { UsuarioId });
		}

		public async Task Actualizar(TiposCuentas tipoCuenta)
		{
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync(@"UPDATE TiposCuentas
												SET Nombre = @Nombre
												WHERE Id = @Id", tipoCuenta);
		}

		public async Task<TiposCuentas> ObtenerPorId(int Id, int UsuarioId)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryFirstOrDefaultAsync<TiposCuentas>(@"
												SELECT Id, Nombre, Orden
												FROM TiposCuentas
												WHERE Id = @Id AND 
													  UsuarioId = @UsuarioId
												ORDER BY Orden",
													  new { Id, UsuarioId}	);
		}

		public async Task Borrar(int Id)
		{
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync("DELETE TiposCuentas " +
				"WHERE Id = @Id", new { Id });
		}

		public async Task Ordenar(IEnumerable<TiposCuentas> tipoCuentasOrdenados)
		{
			var query = "UPDATE TiposCuentas SET " +
				"Orden = @Orden where Id=@Id;";
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync(query, tipoCuentasOrdenados);
		}

	}
}
