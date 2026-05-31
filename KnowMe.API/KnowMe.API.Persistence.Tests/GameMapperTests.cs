using System.Text.Json;
using FluentAssertions;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Domain.Enums;
using KnowMe.API.Persistence.Mapping;
using KnowMe.API.Persistence.Records;

namespace KnowMe.API.Persistence.Tests;

public class GameMapperTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void RoundTrip_ThroughRecordAndJson_PreservesAggregateAndBehaviour()
    {
        var original = BuildPlayedGame(out var player1, out var player2);

        // Map domain -> record -> JSON -> record -> domain, as the repository does.
        var record = GameMapper.ToRecord(original);
        var json = JsonSerializer.Serialize(record, JsonOptions);
        var deserialised = JsonSerializer.Deserialize<GameRecord>(json, JsonOptions)!;
        var rebuilt = GameMapper.ToDomain(deserialised);

        // Identity and lifecycle state preserved.
        rebuilt.Id.Should().Be(original.Id);
        rebuilt.Name.Should().Be(original.Name);
        rebuilt.Status.Should().Be(GameStatus.Started);
        rebuilt.CurrentQuestionPhase.Should().Be(QuestionPhase.Review);
        rebuilt.CurrentQuestionId.Should().Be(original.CurrentQuestionId);
        rebuilt.Players.Select(p => p.Id).Should().BeEquivalentTo(original.Players.Select(p => p.Id));

        // The question back-reference must be rewired so Answered (which reads Game.Players) works.
        var rebuiltQuestion = rebuilt.Questions.Single(q => q.Id == rebuilt.CurrentQuestionId);
        rebuiltQuestion.Answered.Should().BeTrue();

        // Behaviour survives the round trip: scores recompute identically.
        var originalResults = original.GetResults();
        var rebuiltResults = rebuilt.GetResults();

        foreach (var player in new[] { player1, player2 })
        {
            rebuiltResults.Scores.Single(s => s.UserId == player.Id).TotalScore
                .Should().Be(originalResults.Scores.Single(s => s.UserId == player.Id).TotalScore);
        }
    }

    private static Game BuildPlayedGame(out User player1, out User player2)
    {
        player1 = User.Create("player1").Value;
        player2 = User.Create("player2").Value;

        var game = Game.Create("Round trip game", player1).Value;
        game.AddPlayer(player2);

        var v1 = new Dictionary<char, string> { { 'A', "Yes" }, { 'B', "No" } };
        game.AddQuestion(Question.Create("First?", false, v1, player1, game).Value);

        var v2 = new Dictionary<char, string> { { 'A', "Left" }, { 'B', "Right" } };
        game.AddQuestion(Question.Create("Second?", true, v2, player2, game).Value);

        game.StartGame();

        // Fully answer the current question so it sits in review with recorded choices/guesses.
        var q = game.Questions.First(x => x.Id == game.CurrentQuestionId);
        game.RecordChoice(player1, [q.AnswerVariants.First(v => v.Notation == 'A')]);
        game.RecordChoice(player2, [q.AnswerVariants.First(v => v.Notation == 'B')]);
        game.RecordGuess(player1, player2, [q.AnswerVariants.First(v => v.Notation == 'B')]);
        game.RecordGuess(player2, player1, [q.AnswerVariants.First(v => v.Notation == 'A')]);

        return game;
    }
}
