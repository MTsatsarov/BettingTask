using Betting.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace Betting.Data.Entities
{
	public class Match:BaseDeletableEntity<Guid>
	{
		public Match()
		{
			this.Id = Guid.NewGuid();
			this.Bets = new HashSet<Bet>();
		}

		public Match(string name, string givenId, DateTime startDate, MatchType matchType)
		{
			this.Name=name;
			this.GivenId=givenId;
			this.StartDate = startDate;
			this.MatchType = matchType;
			this.Bets = new HashSet<Bet>();

		}

		[Required]
		public string Name{ get; set; }

		[Required]
		public string GivenId { get; set; }

		public DateTime?  StartDate{ get; set; }

		public MatchType MatchType { get; set; }

		public Guid EventId { get; set; }

		public virtual Event Event { get; set; }

		public virtual ICollection<Bet> Bets { get; set; }
	}
}
