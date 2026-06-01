import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { advanceQuestion, getResults } from "../../api/gamesApi";
import type { Game } from "../../api/gamesApi";
import { QuestionResultCard } from "./QuestionResultCard";

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

  const result = resultsQuery.data?.questions.find((q) => q.questionId === game.currentQuestionId);
  const moreToCome = game.questions.some((q) => !q.answered && q.id !== game.currentQuestionId);

  return (
    <div className="page">
      <section className="card">
        <span className="badge status-started">{t("play.review")}</span>
      </section>

      {result && <QuestionResultCard result={result} nameOf={nameOf} />}

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
