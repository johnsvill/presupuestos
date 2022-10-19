using Dapper;
using Microsoft.Data.SqlClient;
using Presupuestos.Models;

namespace Presupuestos.Servicios.Usuarios
{
    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string _connectionString;

        public RepositorioUsuarios(IConfiguration configuration)
        {
            this._connectionString = configuration
                .GetConnectionString("PresupuestosConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(this._connectionString);
            
            var id = await connection.QuerySingleAsync<int>(@$"
                INSERT INTO [Usuarios]([Email], [EmailNormalizado], [PasswordHash])
                VALUES(@Email, @EmailNormalizado, @PasswordHash)

                SELECT SCOPE_IDENTITY()", usuario);

            return id;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(this._connectionString);

            return await connection.QuerySingleOrDefaultAsync(@$"
                        SELECT 
                        UsuarioId AS Id, Email, EmailNormalizado, PasswordHash
                        FROM [Usuarios]
                        WHERE [EmailNormalizado] = @emailNormalizado",
                        new {emailNormalizado});
        }
    }
}
