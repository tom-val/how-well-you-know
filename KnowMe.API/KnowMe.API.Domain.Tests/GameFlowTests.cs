using FluentAssertions;
using KnowMe.API.Domain.Entities;

namespace KnowMe.API.Domain.Tests;

public class GameFlowTests
{
    [Fact]
    public void TwoPlayersShouldBeAbleToFinishGame()
    {
        var player1 = User.Create("player1");
        var player2 = User.Create("player2");

        //Create game
        var game = Game.Create("Test game", player1.Value).Value;

        //Player 2 joins
        game.AddPlayer(player2.Value);

        //Question 1
        var question1Variants = new Dictionary<char, string>()
        {
            { 'A', "Blue" },
            { 'B', "Green" },
            { 'C', "Yellow" }
        };
        var question1 = Question.Create("Favourite color?", false, question1Variants, player1.Value, game);
        game.AddQuestion(question1.Value);

        //Question 2
        var question2Variants = new Dictionary<char, string>()
        {
            { 'A', "Vilnius" },
            { 'B', "Kaunas" },
            { 'C', "Klaipeda" }
        };
        var question2 = Question.Create("Best place to be?", true, question2Variants, player2.Value, game);
        game.AddQuestion(question2.Value);

        //Question 3
        var question3Variants = new Dictionary<char, string>()
        {
            { 'A', "Miškas" },
            { 'B', "Jūra" }
        };
        var question3 = Question.Create("Miškas ar jūra?", true, question3Variants, player2.Value, game);
        game.AddQuestion(question3.Value);

        //Start game
        game.StartGame();
    }
}
