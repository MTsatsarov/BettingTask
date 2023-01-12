
using Betting.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace Betting.Data.Entities
{
	public class Bet:BaseDeletableEntity<Guid>
	{
		public Bet()
		{
			this.Id = Guid.NewGuid();
			this.Odds = new HashSet<Odd>();

		}

		public Bet(string name, string givenId, bool isLive):base()
		{
			Name = name;
			GivenId = givenId;
			IsLive = isLive;
			this.Odds = new HashSet<Odd>();

		}

		[Required]
		public string Name { get; set; }

		[Required]
		public string GivenId { get; set; }

		public bool IsLive { get; set; }

		public Guid MatchId { get; set; }

		public virtual Match Match { get; set; }

		public virtual ICollection<Odd> Odds { get; set; }

	}
}
