using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betting.Services.Models
{
	public class MatchesUpdateNotifier
	{
		private ICollection<MatchResponseModel> matches = new List<MatchResponseModel>();
		public ICollection<MatchResponseModel> Matches
		{
			get => this.matches; set
			{
				this.matches = value;
				this.OnMatchesChanged?.Invoke(this, this.Matches);
			}
		}

		public event EventHandler<ICollection<MatchResponseModel>> OnMatchesChanged;
	}
}
