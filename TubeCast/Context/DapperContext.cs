using MySqlConnector;
using System.Data;

namespace TubeCast.Context
{
	public class DapperContext
	{
		//private readonly IConfiguration _configuration;
		private readonly string _connectionString;
		public DapperContext(/*IConfiguration configuration*/)
		{
			//_configuration = configuration;
			//_connectionString = _configuration.GetConnectionString("SqlConnection")!;
			_connectionString = Secrets.GetConnectionString();
		}
		public MySqlConnection CreateConnection() => new(_connectionString);
	}
}
