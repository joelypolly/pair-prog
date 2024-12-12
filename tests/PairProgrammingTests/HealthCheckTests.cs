using System.Collections;
using Microsoft.EntityFrameworkCore;
using PairProgrammingApi.Modules.HealthCheck;
using PairProgrammingApi.Modules.HealthCheck.Models;
using PairProgrammingTests.DataAccess;
using Xunit;
using Xunit.Abstractions;

namespace PairProgrammingTests;

public class HealthCheckTests : IClassFixture<DatabaseFixture>
{
    private readonly ITestOutputHelper _testOutputHelper;

    public HealthCheckTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task CheckCanCreateData()
    {
        await using var context = DatabaseFixture.CreateContext();
        await context.Database.BeginTransactionAsync();
        context.HealthChecks.Add(new (){ CheckSuccessful = true });
        await context.SaveChangesAsync();
        
        var numberOfHealthChecks = context.HealthChecks.Count();
        Assert.Equal(1, numberOfHealthChecks);
    }
    
    [Fact]
    public async Task CheckApiReturnsTrue()
    {
        await using var context = DatabaseFixture.CreateContext();
        await context.Database.BeginTransactionAsync();
        var beforeCaller = DateTimeOffset.UtcNow;
        var controller = new HealthCheckController(context);
        await controller.CheckHealthAsync();
        
        var result = await context.HealthChecks.SingleOrDefaultAsync();
        Assert.NotNull(result);
        // check that it is something we created
        Assert.True(result.CreatedUtc >= beforeCaller && result.CreatedUtc < DateTimeOffset.UtcNow);
    }

    public List<List<string>> RunFulfimentRules(List<List<string>> rules)
    {
        var result = new List<List<string>>()
        {
        };
        
        var indexOfVisited = new HashSet<int>();
        var currentIndex = 0;
        while (currentIndex < rules.Count())
        {
            if (indexOfVisited.Contains(currentIndex))
            {
                currentIndex++;
                continue;
            }
            
            if (rules[currentIndex].Count > 0)
            {
                var shipTogether = rules[currentIndex].Distinct().ToList();

                var listExhaused = false;
                while (!listExhaused)
                {
                    var matchingShipTogether = rules.FindAll(rule => rule.Intersect(shipTogether).Any());
                    listExhaused = !matchingShipTogether.Any();
                    if (listExhaused)
                    {
                        break;
                    }

                    var shouldExit = true;
                    foreach (var alsoShipTogether in matchingShipTogether)
                    {
                        var index = rules.IndexOf(alsoShipTogether);
                        if (!indexOfVisited.Add(index))
                        {
                            continue;
                        }

                        shouldExit = false;
                        shipTogether.AddRange(alsoShipTogether);
                    }

                    if (shouldExit)
                    {
                        break;
                    }
                }
                
                result.Add(shipTogether.Distinct().ToList());
            }

            indexOfVisited.Add(currentIndex);
            currentIndex++;
        }
        return result;
    }

    [Fact]
    public void FulfillmentRulesMultiRuleTest()
    {
        var rules = new List<List<string>>()
        {
            new List<string>() { "A", "B" },
            new List<string>() { "B", "C" },
            new List<string>() { "C", "D" }
        };

        var foundRules = RunFulfimentRules(rules);
        
        var expectedRules = new List<List<string>>()
        {
            new List<string>() { "A", "B", "C", "D"}
        };
        Assert.Equal(4, foundRules[0].Count());
        foreach (var expectedRule in expectedRules)
        {
            var ruleIndex = foundRules.FindIndex(rule => rule.Intersect(expectedRule).Count() == expectedRule.Count);
            if (ruleIndex >= 0)
            {
                foundRules.Remove(foundRules[ruleIndex]);
            }
        }
        
        Assert.Empty(foundRules);
    }
    
    [Fact]
    public void FulfillmentRulesMultiRuleTest2()
    {
        var rules = new List<List<string>>()
        {
            new List<string>() { "A", "B" },
            new List<string>() { "C", "D" },
            new List<string>() { "D", "E" },
            new List<string>() { "F", "G" }

        };

        var foundRules = RunFulfimentRules(rules);
        
        var expectedRules = new List<List<string>>()
        {
            new List<string>() { "A", "B"},
            new List<string>() {"C", "D", "E"},
            new List<string>() { "F", "G"}

        };
        foundRules.ForEach(i=> _testOutputHelper.WriteLine(string.Join(",", i)));
        
        Assert.Equal(2, foundRules[0].Count());
        Assert.Equal(3, foundRules[1].Count());
        Assert.Equal(2, foundRules[2].Count());
        
        foreach (var expectedRule in expectedRules)
        {
            var ruleIndex = foundRules.FindIndex(rule => rule.Intersect(expectedRule).Count() == expectedRule.Count);
            if (ruleIndex >= 0)
            {
                foundRules.Remove(foundRules[ruleIndex]);
            }
        }
        Assert.Empty(foundRules);
    }
    
    [Fact]
    public void FulfillmentRulesMultiRuleTest3()
    {
        var rules = new List<List<string>>()
        {
            new List<string>() { "A", "B", "C"},
            new List<string>() { "E", "F", "G" },
            new List<string>() {"J", "K" },
            new List<string>() {"C", "D", "E" }

        };

        var foundRules = RunFulfimentRules(rules);
        
        var expectedRules = new List<List<string>>()
        {
            new List<string>() { "A", "B", "C", "D", "E", "F", "G"},
            new List<string>() {"J", "K"},

        };
        foundRules.ForEach(i=> _testOutputHelper.WriteLine(string.Join(",", i)));
        
        Assert.Equal(7, foundRules[0].Count());
        Assert.Equal(2, foundRules[1].Count());
        
        foreach (var expectedRule in expectedRules)
        {
            var ruleIndex = foundRules.FindIndex(rule => rule.Intersect(expectedRule).Count() == expectedRule.Count);
            if (ruleIndex >= 0)
            {
                foundRules.Remove(foundRules[ruleIndex]);
            }
        }
        Assert.Empty(foundRules);
    }
    
    [Fact]
    public void FulfillmentRulesMultiRuleTest4()
    {
        var rules = new List<List<string>>()
        {
            new List<string>() { "A", "B", "C", "C"},
            new List<string>(),
            new List<string>() {"A", "D", "D"},
            new List<string>() {"D", "E"},
            new List<string>() {"F", "G" }
 
        };

        var foundRules = RunFulfimentRules(rules);
        
        var expectedRules = new List<List<string>>()
        {
            new List<string>() { "A", "B", "C", "D", "E"},
            new List<string>() {"F", "G"},

        };
        foundRules.ForEach(i=> _testOutputHelper.WriteLine(string.Join(",", i)));
        
        Assert.Equal(5, foundRules[0].Count());
        Assert.Equal(2, foundRules[1].Count());
        
        foreach (var expectedRule in expectedRules)
        {
            var ruleIndex = foundRules.FindIndex(rule => rule.Intersect(expectedRule).Count() == expectedRule.Count);
            if (ruleIndex >= 0)
            {
                foundRules.Remove(foundRules[ruleIndex]);
            }
        }
        Assert.Empty(foundRules);
    }

    [Fact]
    public void FulfillmentRulesSingleRuleTest()
    {
        var rules = new List<List<string>>()
        {
            new List<string>() { "A", "B" },
            new List<string>() { "B", "C" },
            //new List<string>() { "C", "D" }
        };

        var foundRules = RunFulfimentRules(rules);
        
        var expectedRules = new List<List<string>>()
        {
            new List<string>() { "A", "B", "C", }
        };

        foreach (var expectedRule in expectedRules)
        {
            var ruleIndex = foundRules.FindIndex(rule => rule.Intersect(expectedRule).Count() == expectedRule.Count);
            if (ruleIndex >= 0)
            {
                foundRules.Remove(foundRules[ruleIndex]);
            }
        }
        
        Assert.Empty(foundRules);
    }

    [Fact]
    public void FulfillmentRulesNotEqual()
    {
        var rules = new List<List<string>>()
        {
            new() { "A", "B" },
            new() { "B", "C" },
            new() { "C", "D" }
        };
        
        var foundRules = RunFulfimentRules(rules);
        foundRules.ForEach(rule => _testOutputHelper.WriteLine(string.Join(",", rule)));

        var expectedRules = new List<List<string>>()
        {
            new() { "A", "B" }
        };

        foreach (var expectedRule in expectedRules)
        {
            var ruleIndex = foundRules.FindIndex(rule => rule.Intersect(expectedRule).Count() == rule.Count);
            if (ruleIndex > -1)
            {
                _testOutputHelper.WriteLine("Found a rule");
                foundRules.Remove(foundRules[ruleIndex]);
            }
        }

        Assert.NotEmpty(foundRules);
    }
    
    [Fact]
    public async Task Play()
    {
        var players = new List<string>(){"Joe", "Jill"};
        var numberOfCards = 52;
        PlayCards(players, numberOfCards);

    }
    
    private class Player(string name)
    {
        public string Name { get; } = name;

        public Stack<int> Cards { get; } = new Stack<int>();
        public List<int> CardsWon { get; } = new List<int>();
        public int? PlayCard()
        {
            if (Cards.TryPop(out var card))
            {
                return card;
            }
            return null;
        }
        public void AddWinningCards(List<int?> cards)
        {
            CardsWon.AddRange(cards.Where(i=> i.HasValue).Select(i=> i!.Value));
        }
        public int Score => CardsWon.Count;
        public int HighestCard => CardsWon.Max();
    }

    public string PlayCards(List<string> playerNames, int numberOfCards)
    {
        var cards = new Stack<int>(Shuffle(numberOfCards));
        var players = new List<Player>();
        playerNames.ForEach(playerName => players.Add(new Player(playerName)));

        var playerIndex = 0;
        while (cards.TryPop(out var card))
        {
            players[playerIndex % players.Count].Cards.Push(card);
            playerIndex++;
        }
        
        while(players.Any(player => player.Cards.Count != 0))
        {
            var cardsPlayed = players.Select(player => player.PlayCard()).ToList();
            var winningPlayerIndex = cardsPlayed.IndexOf(cardsPlayed.Max());
            players[winningPlayerIndex].AddWinningCards(cardsPlayed);
        }

        string winner;
        var maxScore = players.Select(i => i.Score).Max();
        if (players.Count(i => i.Score == maxScore) > 1)
        {
            _testOutputHelper.WriteLine($"Entered Tie breaker");
            winner = players.Where(i => i.Score == maxScore).MaxBy(i=> i.HighestCard)!.Name;
        }
        else
        {
            winner = players.MaxBy(i => i.Score)!.Name;
        }
        _testOutputHelper.WriteLine($"Winner is {winner}");
        players.ForEach(player => _testOutputHelper.WriteLine($"{player.Name}: Score is {player.Score}"));
        return winner;
    }

    // produce a list of n values that are between 1 and 52 in a random order
    public List<int> Shuffle(int numberOfCards)
    {
        var shuffled = new List<int>();
        for (var i = 1; i <= numberOfCards; i++)
        {
            shuffled.Add(i);
        }
        var shuffledList = shuffled.ToArray();
        Random.Shared.Shuffle(shuffledList);
        return shuffledList.ToList();
    }
}