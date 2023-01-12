using Betting.Data;
using Betting.Data.Entities;
using Betting.Services.Interfaces;
using Betting.Services.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace Betting.Services
{
	
	public class MatchService : IMatchService
	{
		private BettingContext db;
		private readonly string[] previewMarkets =
		new string[] { "Match Winner", "Map Advantage", "Total Maps Played" };

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
				dbSport.Events = this.GetXmlEvents(sport);

				await this.db.Sports.AddAsync(dbSport);
			}
			else
			{
				var events = this.GetXmlEvents(sport);
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
				var currMatch = new MatchResponseModel();
				currMatch.Name = match.Name;
				currMatch.StartDate = match.StartDate.Value;
				currMatch.MatchId = match.Id;

				var matchMarkets = match.Bets.Where(b => this.previewMarkets.Contains(b.Name)).ToList();

				if (!matchMarkets.Any())
				{
					matchesModel.Matches.Add(currMatch);
					continue;
				}
				else
				{
					foreach (var market in matchMarkets)
					{
						var currMarket = new BetResponseModel();
						currMarket.Name = market.Name;
						currMarket.IsLive = market.IsLive;

						if(!market.Odds.Any(x=>x.SpecialBetValue != null))
						{
							var oddresponseList = market.Odds.Select(o => new OddResponseModel()
							{
								Name = o.Name,
								Value = o.Value,
							});
							currMarket.Odds = oddresponseList.ToList();
						}
						else
						{
							var odds = market.Odds.GroupBy(o => o.SpecialBetValue).Select(x => new OddResponseModel()
							{
								Name = x.FirstOrDefault().Name,
								SpecialBetValue = x.FirstOrDefault().SpecialBetValue,
								Value = x.FirstOrDefault().Value
							}).Take(1).ToList();

							currMarket.Odds = odds.ToList();
						}
						currMatch.Markets.Add(currMarket);
					}
				}
				matchesModel.Matches.Add(currMatch);
				
			}
			return matchesModel;
		}

		public async Task<MatchResponseModel> GetMatchById(Guid matchId)
		{
			var dbMatch = await this.db.Matches.FirstOrDefaultAsync(m => m.Id == matchId);

			if (dbMatch is null)
			{
				throw new InvalidOperationException("Invalid match id.");
			}

			var matchModel = new MatchResponseModel();
			matchModel.Name = dbMatch.Name;
			matchModel.StartDate = dbMatch.StartDate.Value;
			matchModel.MatchId = dbMatch.Id;

			var markets = dbMatch.Bets.Select(b => new BetResponseModel()
			{
				IsLive = b.IsLive,
				Name = b.Name,
				Odds = b.Odds.Select(o => new OddResponseModel()
				{
					Name = o.Name,
					SpecialBetValue = o.SpecialBetValue,
					Value = o.Value
				}).ToList()
			}).ToList();

			matchModel.Markets = markets.ToList();

			return matchModel;
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
				var dbEvent = dbSport.Events.FirstOrDefault(e=>e.GivenId == currEvent.GivenId);

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

		private List<Odd> GetXmlOdds(XElement bet)
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

		private List<Bet> GetXmlMarkets(XElement match)
		{
			var bets = match.Descendants("Bet").ToList();
			var betList = new List<Bet>();

			foreach (var bet in bets)
			{
				var betName = bet.Attributes().FirstOrDefault(x => x.Name == "Name").Value;
				var betId = bet.Attributes().FirstOrDefault(x => x.Name == "ID").Value;
				var betIsLive = bool.Parse(bet.Attributes().FirstOrDefault(x => x.Name == "IsLive").Value);

				var newBet = new Bet(betName, betId, betIsLive);

				newBet.Odds = this.GetXmlOdds(bet);


				betList.Add(newBet);
			}

			return betList;
		}

		private List<Match> GetXmlMatches(XElement currEvent)
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


				currMatch.Bets = this.GetXmlMarkets(match);

				matchList.Add(currMatch);
			}
			return matchList;
		}

		private List<Event> GetXmlEvents(XElement sport)
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
				newEvent.Matches = this.GetXmlMatches(currEvent);
				eventList.Add(newEvent);
			}

			return eventList;
		}
	}
}
