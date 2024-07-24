using Dapper.Contrib.Extensions;

namespace TubeCast.Dapper
{
    [Table("seen")]
	public class Seen
	{
        public int Id { get; set; }
        public string Ytid { get; set; } = string.Empty;
        public DateTime Dateseen { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
