using Betting.Data.Common;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Betting.Data.Entities
{
	public class Event: BaseDeletableEntity<Guid>
	{
		public Event()
		{
			this.Id = Guid.NewGuid();
			this.Matches = new HashSet<Match>();
		}

		public Event(string givenId, string name, bool isLive, string categoryId):base()
		{
			GivenId = givenId;
			Name = name;
			IsLive = isLive;
			CategoryId = categoryId;
			this.Matches = new HashSet<Match>();

		}

		public string GivenId { get; set; }

		public string Name { get; set; }

		public bool IsLive { get; set; }

		public string CategoryId { get; set; }

		public Guid SportId { get; set; }

		public virtual Sport Sport { get; set; }

		public virtual ICollection<Match> Matches { get; set; }
	}
}
