using Dapper;
using Dapper.Contrib.Extensions;
using MySqlConnector;
using TubeCast.Context;
using TubeCast.Dapper;

namespace TubeCast.Data
{
	public interface IDataProvider
	{
		public Task<Settings> GetSettings();
		public Task<IEnumerable<Seen>> GetSeen();
		public Task<IEnumerable<Seen>> GetSeenFromListOfIDs(IEnumerable<string> ids);
		void InsertNewSeen(List<Seen> newItems);
	}

	public class DataProvider : IDataProvider
	{
		private readonly DapperContext _context;

		public DataProvider(DapperContext context)
		{
			_context = context;
		}

		public async Task<Settings> GetSettings()
		{
			using MySqlConnection conn = _context.CreateConnection();
			return await conn.QuerySingleAsync<Settings>("SELECT api, domain, path FROM settings");
		}

		public async Task<IEnumerable<Seen>> GetSeen()
		{
			using MySqlConnection conn = _context.CreateConnection();
			return await conn.QueryAsync<Seen>("SELECT id, ytid, dateseen, location FROM seen");
		}

		public async Task<IEnumerable<Seen>> GetSeenFromListOfIDs(IEnumerable<string> ids)
		{
			if (!ids.Any())
				return [];

			using MySqlConnection conn = _context.CreateConnection();
			return await conn.QueryAsync<Seen>($"SELECT id, ytid, dateseen, location FROM seen WHERE ytid IN ({string.Join(",", ids.Select(x => $"'{x}'"))})");
		}

		public async void InsertNewSeen(List<Seen> newItems)
		{
			if (newItems == null || newItems.Count == 0)
				return;

			using MySqlConnection conn = _context.CreateConnection();
			await conn.InsertAsync(newItems);
			return;
		}
	}
}
