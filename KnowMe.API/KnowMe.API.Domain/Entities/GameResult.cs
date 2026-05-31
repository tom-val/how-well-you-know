namespace KnowMe.API.Domain.Entities;

public class GameResult
{
    public Guid GameId { get; private set; }
    public IReadOnlyList<GameUserScore> Scores { get; private set; }

    private GameResult(Guid gameId, IReadOnlyList<GameUserScore> scores)
    {
        GameId = gameId;
        Scores = scores;
    }

    internal static GameResult Create(Guid gameId, IEnumerable<(Guid UserId, int TotalScore)> userTotals)
    {
        var ordered = userTotals.OrderByDescending(t => t.TotalScore).ToList();

        var scores = new List<GameUserScore>();
        var rank = 0;
        int? previousScore = null;
        for (var i = 0; i < ordered.Count; i++)
        {
            var (userId, totalScore) = ordered[i];

            //Players sharing a score share a rank; the next distinct score skips the tied positions
            if (previousScore is null || totalScore != previousScore)
            {
                rank = i + 1;
            }

            scores.Add(new GameUserScore(userId, totalScore, rank));
            previousScore = totalScore;
        }

        return new GameResult(gameId, scores);
    }
}
