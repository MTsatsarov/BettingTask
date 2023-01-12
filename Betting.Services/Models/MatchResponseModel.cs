namespace Betting.Services.Models
{
	public class MatchResponseModel
	{
		public string Name { get; set; }

		public DateTime StartDate{ get; set; }

		public IEnumerable<BetResponseModel> Markets { get; set; }
	}
}
