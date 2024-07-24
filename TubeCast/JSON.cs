namespace TubeCast.Channel
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class Default
	{
		public string Url { get; set; } = string.Empty;
		public int Width { get; set; }
		public int Height { get; set; }
	}

	public class High
	{
		public string Url { get; set; } = string.Empty;
		public int Width { get; set; }
		public int Height { get; set; }
	}

	public class Item
	{
		public string Kind { get; set; } = string.Empty;
		public string Etag { get; set; } = string.Empty;
		public string Id { get; set; } = string.Empty;
		public Snippet? Snippet { get; set; }
	}

	public class Localized
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}

	public class Medium
	{
		public string Url { get; set; } = string.Empty;
		public int Width { get; set; }
		public int Height { get; set; }
	}

	public class PageInfo
	{
		public int TotalResults { get; set; }
		public int ResultsPerPage { get; set; }
	}

	public class Root
	{
		public string Kind { get; set; } = string.Empty;
		public string Etag { get; set; } = string.Empty;
		public PageInfo? PageInfo { get; set; }
		public List<Item>? Items { get; set; }
	}

	public class Snippet
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string CustomUrl { get; set; } = string.Empty;
		public DateTime PublishedAt { get; set; }
		public Thumbnails? Thumbnails { get; set; }
		public Localized? Localized { get; set; }
		public string Country { get; set; } = string.Empty;
	}

	public class Thumbnails
	{
		public Default? Default { get; set; }
		public Medium? Medium { get; set; }
		public High? High { get; set; }
	}
}

namespace TubeCast.Search
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class Default
	{
		public string Url { get; set; } = string.Empty;
		public int Width { get; set; }
		public int Height { get; set; }
	}

	public class High
	{
		public string Url { get; set; } = string.Empty;
		public int Width { get; set; }
		public int Height { get; set; }
	}

	public class Id
	{
		public string Kind { get; set; } = string.Empty;
		public string VideoId { get; set; } = string.Empty;
	}

	public class Item
	{
		public string Kind { get; set; } = string.Empty;
		public string Etag { get; set; } = string.Empty;
		public Id? Id { get; set; }
		public Snippet? Snippet { get; set; }
	}

	public class Medium
	{
		public string Url { get; set; } = string.Empty;
		public int Width { get; set; }
		public int Height { get; set; }
	}

	public class PageInfo
	{
		public int TotalResults { get; set; }
		public int ResultsPerPage { get; set; }
	}

	public class Root
	{
		public string Kind { get; set; } = string.Empty;
		public string Etag { get; set; } = string.Empty;
		public string NextPageToken { get; set; } = string.Empty;
		public string RegionCode { get; set; } = string.Empty;
		public PageInfo? PageInfo { get; set; }
		public List<Item>? Items { get; set; }
	}

	public class Snippet
	{
		public DateTime PublishedAt { get; set; }
		public string ChannelId { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public Thumbnails? Thumbnails { get; set; }
		public string ChannelTitle { get; set; } = string.Empty;
		public string LiveBroadcastContent { get; set; } = string.Empty;
		public DateTime? PublishTime { get; set; }
	}

	public class Thumbnails
	{
		public Default? Default { get; set; }
		public Medium? Medium { get; set; }
		public High? High { get; set; }
	}


}