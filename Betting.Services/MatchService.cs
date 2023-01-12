using Betting.Data;
using Betting.Data.Entities;
using Betting.Services.Interfaces;
using Betting.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace Betting.Services
{
	public class MatchService : IMatchService
	{
		private BettingContext db;

		public MatchService(BettingContext db)
		{
			this.db = db;
		}

		public async Task GetBettingInfo()
		{
			var endpoint = "https://sports.ultraplay.net/sportsxml?clientKey=9C5E796D-4D54-42FD-A535-D7E77906541A&sportId=2357&days=7";
			var client = new HttpClient();
			var responseMessage = await client.GetAsync(endpoint);

			if (!responseMessage.IsSuccessStatusCode)
			{
				//log error;
				return;
			}

			var xmlAsString = await responseMessage.Content.ReadAsStringAsync();
			var doc = XDocument.Parse(xmlAsString);

			var sport = doc.Root.Descendants("Sport").FirstOrDefault();
			var sportGivenId = sport?.Attributes().FirstOrDefault(x => x.Name == "ID").Value;
			var sportName = sport?.Attributes().FirstOrDefault(x => x.Name == "Name").Value;

			var dbSport = await this.db.Sports.FirstOrDefaultAsync(x => x.GivenId == sportGivenId);

			if (dbSport is null)
			{
				dbSport = new Sport();
				dbSport.GivenId = sportGivenId;
				dbSport.Name = sportName;
				dbSport.Events = this.GetEvents(sport);

				await this.db.Sports.AddAsync(dbSport);
			}
			else
			{
				var events = this.GetEvents(sport);
				this.UpdateSport(dbSport, events);

				this.db.Sports.Update(dbSport);
			}
			await this.db.SaveChangesAsync();
		}

		public async Task<MatchesResponseModel> GetMatchesForTimePeriod(int hours)
		{
			var matchesModel = new MatchesResponseModel();
			var matches = await this.db.Matches
				.Where(x => x.StartDate.Value >= DateTime.UtcNow
					&& x.StartDate.Value <= DateTime.UtcNow.AddHours(hours))
				.ToListAsync();

			foreach (var match in matches)
			{
				var currMatchModel = new MatchResponseModel()
				{
					Name = match.Name,
					StartDate = match.StartDate.Value,
				};

				if (!match.Bets.Any(x => x.Odds.Any(x => x.SpecialBetValue != null)))
				{
					var currMatchBets = match.Bets.Select(x => new BetResponseModel()
					{
						Name = x.Name,
						IsLive = x.IsLive,
						Odds = x.Odds.Select(x => new OddResponseModel()
						{
							Name = x.Name,
							SpecialBetValue = null,
							Value = x.Value,
						}),
					});
					currMatchModel.Markets = currMatchBets.ToList();
				}
				else
				{
					var currMatchBets = match.Bets.Where(x=>x.Odds.Any(x=>x.SpecialBetValue!=null)).ToList();

					foreach (var bet in currMatchBets)
					{
						var a = bet.Odds.GroupBy(x => x.SpecialBetValue);
						;
						//var odds = .Select((x, y) => new OddResponseModel()
						//{
						//	Name = x.FirstOrDefault().Name,
						//	SpecialBetValue = x.FirstOrDefault().SpecialBetValue
						//}).ToList();

					}
					/*x => new BetResponseModel()*/
					//{
					//	Name = x.Name,
					//	IsLive = x.IsLive,
					//	Odds = x.Odds.GroupBy(x => x.SpecialBetValue).Select(x => new OddResponseModel()
					//	{
					//		SpecialBetValue = x.FirstOrDefault(x => x.SpecialBetValue != null).SpecialBetValue,
					//		Name = x.FirstOrDefault(x => x.SpecialBetValue != null).Name,
					//		Value = x.FirstOrDefault(x => x.SpecialBetValue != null).Value,

					//	}).Take(1).ToList()
					//}).FirstOrDefault();

					//currMatchModel.Markets = new List<BetResponseModel>() {
					//	currMatchBets,
					//};
					continue;
				}
				matchesModel.Matches.Add(currMatchModel);

			}

			return matchesModel;
		}

		private void UpdateSport(Sport dbSport, List<Event> events)
		{
			foreach (var currEvent in events)
			{
				if (!dbSport.Events.Any(x => x.GivenId == currEvent.GivenId))
				{
					dbSport.Events.Add(currEvent);
					continue;
				}
				var dbEvent = dbSport.Events.FirstOrDefault();
				foreach (var currMatch in currEvent.Matches)
				{
					var dbMatch = dbEvent.Matches.FirstOrDefault(x => x.GivenId == currMatch.GivenId);
					if (dbMatch == null)
					{
						currEvent.Matches.Add(currMatch);
						continue;
					}

					if (currMatch.StartDate != dbMatch.StartDate)
					{
						dbMatch.StartDate = currMatch.StartDate;
					}

					if (currMatch.MatchType != dbMatch.MatchType)
					{
						dbMatch.MatchType = currMatch.MatchType;
					}

					foreach (var bet in currMatch.Bets)
					{
						var currBet = dbMatch
							.Bets
							.Where(x => x.GivenId == bet.GivenId)
							.FirstOrDefault();

						var dbOddList = new List<Odd>();

						if (currBet is not null && currBet.Odds.Any())
						{
							dbOddList = currBet.Odds.ToList();
						}

						foreach (var odd in bet.Odds)
						{
							if (dbOddList.Any(x => x.GivenId == odd.GivenId))
							{
								var dbOdd = dbOddList.FirstOrDefault(x => x.GivenId == odd.GivenId);

								if (dbOdd.Value != odd.Value)
								{
									dbOdd.Value = odd.Value;
								}
							}
						}
					}
				}
			}
		}

		private List<Odd> GetOdds(XElement bet)
		{
			var odds = bet.Descendants("Odd").ToList();
			var oddList = new List<Odd>();
			foreach (var odd in odds)
			{
				var oddName = odd.Attributes().FirstOrDefault(x => x.Name == "Name").Value;
				var oddId = odd.Attributes().FirstOrDefault(x => x.Name == "ID").Value;
				var oddValue = odd.Attributes().FirstOrDefault(x => x.Name == "Value").Value;
				var hasSpecialbet = odd.Attributes().FirstOrDefault(x => x.Name == "SpecialBetValue");

				string specialBetValue = null;
				if (hasSpecialbet != null)
				{
					specialBetValue = hasSpecialbet.Value;
				}
				oddList.Add(new Odd(oddName, oddId, oddValue, specialBetValue));
			}

			return oddList;
		}

		private List<Bet> GetBets(XElement match)
		{
			var bets = match.Descendants("Bet").ToList();
			var betList = new List<Bet>();

			foreach (var bet in bets)
			{
				var betName = bet.Attributes().FirstOrDefault(x => x.Name == "Name").Value;
				var betId = bet.Attributes().FirstOrDefault(x => x.Name == "ID").Value;
				var betIsLive = bool.Parse(bet.Attributes().FirstOrDefault(x => x.Name == "IsLive").Value);

				var newBet = new Bet(betName, betId, betIsLive);

				newBet.Odds = this.GetOdds(bet);


				betList.Add(newBet);
			}

			return betList;
		}

		private List<Match> GetMatches(XElement currEvent)
		{
			var matches = currEvent.Descendants("Match").ToList();
			var matchList = new List<Match>();
			foreach (var match in matches)
			{
				var matchName = match.Attributes().FirstOrDefault(x => x.Name == "Name").Value;
				var matchId = match.Attributes().FirstOrDefault(x => x.Name == "ID").Value;
				Enum.TryParse<MatchType>(match.Attributes().FirstOrDefault(x => x.Name == "MatchType").Value, true, out MatchType matchType);
				var startDate = DateTime.Parse(match.Attributes().FirstOrDefault(x => x.Name == "StartDate").Value);

				var currMatch = new Match(matchName, matchId, startDate, matchType);


				currMatch.Bets = this.GetBets(match);

				matchList.Add(currMatch);
			}
			return matchList;
		}

		private List<Event> GetEvents(XElement sport)
		{
			var events = sport.Descendants("Event").ToList();
			var eventList = new List<Event>();
			foreach (var currEvent in events)
			{
				var eventName = currEvent.Attributes().FirstOrDefault(x => x.Name == "Name").Value;
				var eventGivenId = currEvent.Attributes().FirstOrDefault(x => x.Name == "ID").Value;
				var eventIsLive = bool.Parse(currEvent.Attributes().FirstOrDefault(x => x.Name == "IsLive").Value);
				var categoryId = currEvent.Attributes().FirstOrDefault(x => x.Name == "CategoryID").Value;

				var newEvent = new Event(eventGivenId, eventName, eventIsLive, categoryId);
				newEvent.Matches = this.GetMatches(currEvent);
				eventList.Add(newEvent);
			}

			return eventList;
		}


	}
}
