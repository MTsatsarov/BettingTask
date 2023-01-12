namespace Betting.Services.Models
{
	public class BetResponseModel
	{
		public bool IsLive { get; set; }

		public string Name { get; set; }
		public IEnumerable<OddResponseModel> Odds { get; set; }
	}
}
