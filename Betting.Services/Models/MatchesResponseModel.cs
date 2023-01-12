namespace Betting.Services.Models
{
	public class MatchesResponseModel
	{
		public MatchesResponseModel()
		{
			Matches = new List<MatchResponseModel>();
		}
		public ICollection<MatchResponseModel> Matches { get; set; }
	}
}
