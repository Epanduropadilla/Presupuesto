using Dapper;
using Microsoft.Data.SqlClient;
using Presupuesto.Models;
using System.Reflection.Metadata.Ecma335;

namespace Presupuesto.Servicios
{
	public interface IRepositorioCuentas
	{
		Task Actualizar(CuentaCreacionViewModel cuenta);
		Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
		Task Crear(Cuenta cuenta);
		Task<Cuenta> ObtenerPorId(int id, int usuarioId);
	}
	public class RepositorioCuentas: IRepositorioCuentas
	{
		private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
			connectionString = configuration.GetConnectionString("DefaultConnection");
        }
		public async Task Crear(Cuenta cuenta)
		{
			using var connection = new SqlConnection(connectionString);
			var id = await connection.QuerySingleAsync<int>(
				@"INSERT INTO Cuentas (Nombre, TipoCuentaID, Balance,Descripcion)
                   VALUES (@Nombre, @TipoCuentaID, @Balance,@Descripcion);
                   SELECT SCOPE_IDENTITY();", cuenta);

			cuenta.Id = id;
		}

		public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
		{ 
		    using var connection = new SqlConnection(connectionString);
			return await connection.QueryAsync<Cuenta>(@"
						SELECT c.Id, c.Nombre, Balance, tc.Nombre[TipoCuenta]
						FROM Cuentas c INNER JOIN TiposCuentas tc
								On tc.Id = c.TipoCuentaId
						WHERE tc.UsuarioId = @UsuarioId
						ORDER BY tc.Orden", new { usuarioId });
		}

		public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryFirstOrDefaultAsync<Cuenta>(
				@"SELECT c.Id, c.Nombre, Balance, Descripcion,tc.Id
				  FROM Cuentas c INNER JOIN TiposCuentas tc
						On tc.Id = c.TipoCuentaId
				  WHERE tc.UsuarioId = @UsuarioId AND c.Id = @Id",
				new { id, usuarioId});
		}

		public async Task Actualizar(CuentaCreacionViewModel cuenta)
		{
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync(
				@"UPDATE Cuentas
                  SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion,
					  TipoCuentaId = @TipoCuentaId
                  WHERE Id = @Id;", cuenta);
		}
    }
}
