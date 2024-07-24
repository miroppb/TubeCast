using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace RssGenerator
{
	[XmlRoot(ElementName = "enclosure")]
	public class Enclosure
	{
		[XmlAttribute(AttributeName = "url")]
		public string Url { get; set; } = string.Empty;
		[XmlAttribute(AttributeName = "type")]
		public string Type { get; set; } = string.Empty;
	}
	[XmlRoot(ElementName = "item")]
	public class Item
	{
		[XmlElement(ElementName = "title")]
		public string Title { get; set; } = string.Empty;
		[XmlElement(ElementName = "description")]
		public string Description { get; set; } = string.Empty;
		[XmlElement(ElementName = "pubDate")]
		public DateTime PubDate { get; set; }
		[XmlElement(ElementName = "enclosure")]
		public Enclosure Enclosure { get; set; } = new();
	}
	[XmlRoot(ElementName = "image")]
	public class Image
	{

		[XmlElement(ElementName = "url")]
		public string Url { get; set; } = string.Empty;

		[XmlElement(ElementName = "title")]
		public string Title { get; set; } = string.Empty;

		[XmlElement(ElementName = "link")]
		public string Link { get; set; } = string.Empty;
	}
	[XmlRoot(ElementName = "channel")]
	public class Channel
	{
		[XmlElement(ElementName = "title")]
		public string Title { get; set; } = string.Empty;
		[XmlElement(ElementName = "description")]
		public string Description { get; set; } = string.Empty;
		[XmlElement(ElementName = "category")]
		public string Category { get; set; } = string.Empty;
		[XmlElement(ElementName = "pubDate")]
		public DateTime PubDate { get; set; }
		[XmlElement(ElementName = "image")]
		public List<Image> Image { get; set; } = [];
		[XmlElement(ElementName = "item")]
		public List<Item> Item { get; set; } = [];
	}
	[XmlRoot(ElementName = "rss")]
	public class Rss
	{
		[XmlElement(ElementName = "channel")]
		public Channel Channel { get; set; } = new();
		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; } = string.Empty;
	}
}
