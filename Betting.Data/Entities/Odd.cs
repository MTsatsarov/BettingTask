
using Betting.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace Betting.Data.Entities
{
	public class Odd:BaseDeletableEntity<Guid>
	{
		public Odd()
		{
			this.Id = Guid.NewGuid();
		}

		public Odd(string name, string givenId, string value,string specialBetValue) : base()
		{
			this.Name = name;
			this.GivenId = givenId;
			this.Value = value;
			this.SpecialBetValue = specialBetValue;
		}

		[Required]
		public string Name { get; set; }

		[Required]
		public string GivenId { get; set; }

		public string? Value { get; set; }

		public string? SpecialBetValue { get; set; }

		public Guid BetId { get; set; }

		public virtual Bet Bet { get; set; }
	}
}
