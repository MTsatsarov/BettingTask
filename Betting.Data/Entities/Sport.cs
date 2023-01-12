using Betting.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace Betting.Data.Entities
{
	public class Sport:BaseDeletableEntity<Guid>
	{
		public Sport()
		{
			this.Id = Guid.NewGuid();
			this.Events = new HashSet<Event>();
		}

		[Required]
		public string GivenId { get; set; }

		[Required]
		public string Name { get; set; }

		public virtual ICollection<Event> Events{ get; set; } 
	}
}
