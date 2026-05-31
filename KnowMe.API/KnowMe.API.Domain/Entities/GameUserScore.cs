namespace KnowMe.API.Domain.Entities;

public class GameUserScore
{
    public Guid UserId { get; private set; }
    public int TotalScore { get; private set; }
    public int Rank { get; private set; }

    internal GameUserScore(Guid userId, int totalScore, int rank)
    {
        UserId = userId;
        TotalScore = totalScore;
        Rank = rank;
    }
}
