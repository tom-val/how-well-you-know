import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { advanceQuestion, getResults } from "../../api/gamesApi";
import type { Game } from "../../api/gamesApi";

export function ReviewPhase({ game }: { game: Game }) {
  const { t } = useTranslation();
  const { enqueueSnackbar } = useSnackbar();
  const queryClient = useQueryClient();

  const resultsQuery = useQuery({
    queryKey: ["results", game.id, game.currentQuestionId],
    queryFn: () => getResults(game.id),
  });

  const advanceMutation = useMutation({
    mutationFn: () => advanceQuestion(game.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["game", game.id] }),
    onError: (err) =>
      enqueueSnackbar(err instanceof Error ? err.message : t("common.error"), { variant: "error" }),
  });

  const nameOf = (userId: string) =>
    game.players.find((p) => p.id === userId)?.userName ?? userId.slice(0, 8);

  const question = game.questions.find((q) => q.id === game.currentQuestionId);
  const result = resultsQuery.data?.questions.find((q) => q.questionId === game.currentQuestionId);

  const moreToCome = game.questions.some((q) => !q.answered && q.id !== game.currentQuestionId);

  return (
    <div className="page">
      <section className="card">
        <span className="badge status-started">{t("play.review")}</span>
        <h2 className="question-text">{question?.text}</h2>
      </section>

      {result && (
        <section className="card">
          <ul className="result-list">
            {result.players
              .slice()
              .sort((a, b) => b.totalScore - a.totalScore)
              .map((p) => (
                <li key={p.userId}>
                  <div className="result-head">
                    <strong>{nameOf(p.userId)}</strong>
                    <span className="badge muted-badge">{t("play.points", { count: p.totalScore })}</span>
                  </div>
                  <ul className="guess-list">
                    {p.guesses.map((g) => (
                      <li key={g.choiceUserId}>
                        <span>{t("play.guessedFor", { name: nameOf(g.choiceUserId) })}</span>
                        <span className={g.score > 0 ? "correct" : "wrong"}>
                          {g.score > 0 ? "✓" : "✗"} {t("play.points", { count: g.score })}
                        </span>
                      </li>
                    ))}
                  </ul>
                </li>
              ))}
          </ul>
        </section>
      )}

      <section className="card">
        <button
          type="button"
          className="primary block"
          disabled={advanceMutation.isPending}
          onClick={() => advanceMutation.mutate()}
        >
          {moreToCome ? t("play.next") : t("play.finish")}
        </button>
      </section>
    </div>
  );
}
