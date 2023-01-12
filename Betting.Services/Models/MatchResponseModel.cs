namespace Betting.Services.Models
{
	public class MatchResponseModel
	{
		public MatchResponseModel()
		{
			this.Markets = new List<BetResponseModel>();
		}
		public string Name { get; set; }

		public Guid MatchId { get; set; }

		public DateTime StartDate{ get; set; }

		public ICollection<BetResponseModel> Markets { get; set; }
	}
}
