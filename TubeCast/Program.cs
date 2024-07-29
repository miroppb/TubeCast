using System.Timers;
using TubeCast.Context;
using TubeCast.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IDataProvider, DataProvider>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

System.Timers.Timer updateTimer = new() { Interval = TimeSpan.FromDays(1).TotalMilliseconds };
updateTimer.Elapsed += UpdateTimer_Elapsed;
updateTimer.Start();

#if DEBUG
	HttpClient client = new()
	{
		BaseAddress = new("http://localhost:1119/")
	};
	var response = client.GetAsync("tubecast/removeunused");
#endif

static void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
{
	HttpClient client = new()
	{
		BaseAddress = new("http://localhost:1119/")
	};
	var response = client.GetAsync("tubecast/update");
}

app.Run("http://*:1119");