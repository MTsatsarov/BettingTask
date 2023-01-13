namespace Betting.Services.Models
{
	public class BetResponseModel
	{
		public BetResponseModel()
		{
			this.Odds = new List<OddResponseModel>();
		}
		public bool IsLive { get; set; }

		public string Name { get; set; }

		public string GivenId { get; set; }
		public ICollection<OddResponseModel> Odds { get; set; }
	}
}
