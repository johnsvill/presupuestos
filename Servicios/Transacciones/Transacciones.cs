using Dapper;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;

namespace Presupuestos.Servicios.Transacciones
{
    public class Transacciones : ITransacciones
    {
        private readonly string _connectionString;

        public Transacciones(IConfiguration configuration)
        {
            this._connectionString = configuration
                .GetConnectionString("PresupuestosConnection"); 
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(this._connectionString);

            var id = await connection.QuerySingleAsync<int>(@$"
                                 Transacciones_Insertar",
                                 new 
                                 {
                                     transaccion.UsuarioId,
                                     transaccion.FechaTransac,
                                     transaccion.Monto,
                                     transaccion.CategoriaId,
                                     transaccion.CuentaId,
                                     transaccion.Nota
                                 },
                                 commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
