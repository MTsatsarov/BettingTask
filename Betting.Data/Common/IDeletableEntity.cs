
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betting.Data.Common
{
	public interface IDeletableEntity
	{
		bool IsDeleted { get; set; }

		DateTime? DeletedOn { get; set; }
	}
}
