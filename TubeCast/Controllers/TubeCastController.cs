using Microsoft.AspNetCore.Mvc;
using miroppb;
using Newtonsoft.Json;
using RestSharp;
using RssGenerator;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.Serialization;
using TubeCast.Dapper;
using TubeCast.Data;

namespace TubeCast.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TubeCastController(IDataProvider provider) : ControllerBase
	{
		private const string BaseUrl = "https://www.googleapis.com/youtube/v3/";
		private readonly IDataProvider _provider = provider;

		[HttpGet("channelfeed/{channelId}/{maxResults}/{eventType}/{title}")]
		[HttpGet("channelfeed/{channelId}/{maxResults}/{eventType}")]
		[HttpGet("channelfeed/{channelId}/{maxResults}")]
		public async Task<ActionResult<string>> ChannelFeed(string channelId, int maxResults, string eventType = "", string title = "")
		{
			string? userIpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
			string ChannelName = string.Empty;
			string Description = string.Empty;
			string ChannelUrl = string.Empty;
			string ChannelAuthor = string.Empty;
			string ChannelImage = string.Empty;
			Search.Root Search = new();

			//get settings
			Settings settings = await _provider.GetSettings();
			//get channel data
			RestClient client = new(BaseUrl);
			var request = new RestRequest("channels");
			request.AddParameter("key", settings.API, ParameterType.QueryString);
			request.AddParameter("part", "snippet", ParameterType.QueryString);
			if (!channelId.StartsWith('@'))
				request.AddParameter("id", channelId, ParameterType.QueryString);
			else
				request.AddParameter("forHandle", channelId, ParameterType.QueryString);
			Console.WriteLine($"{userIpAddress} Requesting info about channel: {channelId}");
			libmiroppb.Log($"{userIpAddress} Requesting info about channel: {channelId}");
			var response = client.Execute(request);
			if (response.IsSuccessStatusCode)
			{
				Channel.Root ChannelMeta = JsonConvert.DeserializeObject<Channel.Root>(response.Content!)!;
				if (ChannelMeta.PageInfo!.TotalResults == 0)
					return BadRequest("No Channel Found. Please specify either a Channel ID or @Handle");
				ChannelName = ChannelMeta.Items![0].Snippet!.Title;
				ChannelUrl = $"https://youtube.com/user/{ChannelMeta.Items![0].Id}";
				Description = ChannelMeta.Items[0].Snippet!.Description;
				ChannelAuthor = ChannelMeta.Items![0].Snippet!.CustomUrl.ToUpper();
				ChannelImage = ChannelMeta.Items![0].Snippet!.Thumbnails!.Medium!.Url;
				channelId = ChannelMeta.Items[0].Id; //in case of handle
			}
			Search = await UpdatePodcast(channelId, eventType, maxResults, settings);

			var rss = new Rss
			{
				Version = "2.0",
				Channel = new RssGenerator.Channel
				{
					Title = ChannelName,
					Description = Description,
					Category = "self-improvement",
					PubDate = Search.Items!.First().Snippet!.PublishedAt,
					Image = [new() { Url = ChannelImage }],
					Item = []
				}
			};

			IEnumerable<string> allMp3s = Search.Items!.Select(x => $"{settings.Path}\\{x.Id!.VideoId}.mp3");

			foreach (var mp3 in allMp3s)
			{
				var CurItem = Search.Items!.First(x => x.Id!.VideoId == Path.GetFileNameWithoutExtension(mp3)).Snippet!;
				rss.Channel.Item.Add(new Item
				{
					Description = CurItem.Description,
					Title = CurItem.Title,
					PubDate = CurItem.PublishedAt,
					Enclosure = new Enclosure
					{
						Url = $"{settings.Domain}/{Path.GetFileNameWithoutExtension(mp3)}.mp3",
						Type = "audio/mpeg"
					}
				});
			}
			var serializer = new XmlSerializer(typeof(Rss));
			string RssFeedPath = $"{settings.Path}\\{channelId}.rss";
			using (var writer = new StreamWriter(RssFeedPath))
			{
				serializer.Serialize(writer, rss);
			}

			//format into podcast xml
			//return Ok();

			var html = System.IO.File.ReadAllText(RssFeedPath);
			return base.Content(html, "text/xml");
		}

		private async Task<Search.Root> UpdatePodcast(string channelId, string eventType, int maxResults, Settings settings)
		{
			//search channel for videos by criteria
			Search.Root Search = new();
			RestClient client = new(BaseUrl);
			RestRequest request = new("search");
			request.AddParameter("key", settings.API, ParameterType.QueryString);
			request.AddParameter("part", "snippet", ParameterType.QueryString);
			request.AddParameter("channelId", channelId, ParameterType.QueryString);
			request.AddParameter("order", "date", ParameterType.QueryString);
			if (eventType != string.Empty)
				request.AddParameter("eventTipe", eventType, ParameterType.QueryString);
			request.AddParameter("type", "video", ParameterType.QueryString);
			request.AddParameter("videoDuration", "long", ParameterType.QueryString);
			request.AddParameter("publishedAfter", "2024-01-01T00:00:00Z", ParameterType.QueryString);
			request.AddParameter("maxResults", maxResults, ParameterType.QueryString);
			Console.WriteLine($"Sending Request: {string.Join("&", request.Parameters)}");
			libmiroppb.Log($"Sending Request: {string.Join("&", request.Parameters)}");
			RestResponse response = client.Execute(request);
			if (response.IsSuccessStatusCode)
			{
				Search = JsonConvert.DeserializeObject<Search.Root>(response.Content!)!;
			}

			IEnumerable<Seen> seen = await _provider.GetSeenFromListOfIDs(Search.Items!.Select(x => x.Id!.VideoId));
			//for each item, if not in seen, add to list and then save
			List<Seen> newItems = [];
			foreach (var item in Search.Items!)
			{
				if (seen.FirstOrDefault(x => x.Ytid == item.Id!.VideoId) == null)
					newItems.Add(new Seen() { Dateseen = item.Snippet!.PublishTime!.Value, Ytid = item.Id!.VideoId });
			}

			//need to send the list back to the db, and start a conversion process for new tracks
			if (newItems.Count > 0)
			{
				Console.WriteLine($"Got new items: {string.Join(",", newItems.Select(x => x.Ytid))}");
				libmiroppb.Log($"Got new items: {string.Join(",", newItems.Select(x => x.Ytid))}");
				foreach (var item in newItems)
				{
					item.Location = $"{settings.Path}\\{item.Ytid}.mp3";
					Process p = new();
					string temp = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
					p.StartInfo.FileName = "yt-dlp.exe";
					p.StartInfo.Arguments = $"-x --audio-format mp3 https://youtu.be/{item.Ytid} -P \"temp:{temp}\" -P \"{settings.Path}\" -o \"{item.Ytid}.mp3\"";
					p.Start();
					//await p.WaitForExitAsync(); //lets not block the file
				}
				_provider.InsertNewSeen(newItems);
			}

			return Search;
		}

		[HttpGet("download/{file}")]
		public async Task<ActionResult> Download(string file)
		{
			//get settings
			Settings settings = await _provider.GetSettings();

			var bytes = Array.Empty<byte>();

			using (var fs = new FileStream(Path.Combine(settings.Path, file), FileMode.Open, FileAccess.Read))
			{
				var br = new BinaryReader(fs);
				long numBytes = new FileInfo(Path.Combine(settings.Path, file)).Length;
				bytes = br.ReadBytes((int)numBytes);
			}

			string? userIpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
			Console.WriteLine($"{userIpAddress} Requested File: {file}");
			libmiroppb.Log($"{userIpAddress} Requested File: {file}");

			return File(bytes, "audio/mpeg");
		}

		[HttpGet("update")]
		public async Task<ActionResult<string>> Update()
		{
			Process ytdlp = new();
			ytdlp.StartInfo.FileName = "yt-dlp.exe";
			ytdlp.StartInfo.Arguments = "-U"; //update yt-dlp
			ytdlp.Start();
			libmiroppb.Log("Checking for yt-dlp update");

			//get settings
			Settings settings = await _provider.GetSettings();

			//get list of channels from *channel*.rss files
			List<string> channels = Directory.GetFiles(settings.Path, "*.rss").Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
			foreach (string channel in channels)
			{
				_ = UpdatePodcast(channel, string.Empty, 5, settings);
			}
			libmiroppb.Log("Refreshed channels");

			return Ok();
		}

		[HttpGet("removeunused")]
		public async Task<ActionResult<string>> RemoveUnused()
		{
			//get settings
			Settings settings = await _provider.GetSettings();

			//lets read all rss files, and find current podcasts
			List<string> files = [.. Directory.GetFiles(settings.Path, "*.rss")];
			
			List<string> AllPodcasts = [];

			foreach (string file in files)
			{
				XElement rss = XElement.Load(file);
				IEnumerable<IEnumerable<string>> links = rss.Descendants("item").Select(x => x.Descendants("enclosure").Select(x => (string)x.Attribute("url")!));
				List<string> ActualLinks = links.SelectMany(innerEnumerable => innerEnumerable).ToList();
                for (int i = 0; i < ActualLinks.Count; i++)
					ActualLinks[i] = ActualLinks[i].Replace(settings.Domain + "/", settings.Path + "\\");
				AllPodcasts.AddRange(ActualLinks);
            }
			List<string> FilesInDirectory = [.. Directory.GetFiles(settings.Path, "*.mp3")];

			//find files that are in the directory but not in Podcasts
			List<string> FilesToDelete = FilesInDirectory.Except(AllPodcasts).ToList();
			foreach (string file in FilesToDelete)
				System.IO.File.Delete(file);

			List<string> YTIds = FilesToDelete.Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
			IEnumerable<Seen> ItemsToDelete = await _provider.GetSeenFromListOfIDs(YTIds);
			_provider.DeleteSeen(ItemsToDelete);
			
			libmiroppb.Log($"Deleted old podcasts: {string.Join(',', FilesToDelete)}");
			Console.WriteLine($"Deleted old podcasts: {string.Join(',', FilesToDelete)}");

			return Ok();
		}
	}

	[Route("")]
	[ApiController]
	public class RootController : ControllerBase
	{
		[HttpGet]
		public IEnumerable<string> Get() => ["Scary music comes from TubeCast"];
	}
}
