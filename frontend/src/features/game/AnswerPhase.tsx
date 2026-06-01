import { useTranslation } from "react-i18next";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { recordChoice, recordGuess } from "../../api/gamesApi";
import type { Game, Question } from "../../api/gamesApi";
import { useAuth } from "../../hooks/useAuth";
import { AnswerForm } from "./AnswerForm";

export function AnswerPhase({ game, question }: { game: Game; question: Question }) {
  const { t } = useTranslation();
  const { user } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const queryClient = useQueryClient();

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["game", game.id] });
  const onError = (err: unknown) =>
    enqueueSnackbar(err instanceof Error ? err.message : t("common.error"), { variant: "error" });

  const choiceMutation = useMutation({
    mutationFn: (notations: string[]) => recordChoice(game.id, notations),
    onSuccess: invalidate,
    onError,
  });

  const guessMutation = useMutation({
    mutationFn: ({ choiceUserId, notations }: { choiceUserId: string; notations: string[] }) =>
      recordGuess(game.id, choiceUserId, notations),
    onSuccess: invalidate,
    onError,
  });

  const viewer = game.viewer;
  const myId = user?.sub;

  return (
    <div className="page">
      <section className="card">
        <span className="badge status-started">{t("status.Started")}</span>
        <h2 className="question-text">{question.text}</h2>
      </section>

      {!viewer?.hasAnswered ? (
        <section className="card">
          <h3>{t("play.yourAnswer")}</h3>
          <AnswerForm
            question={question}
            submitLabel={t("play.answer")}
            submitting={choiceMutation.isPending}
            onSubmit={(n) => choiceMutation.mutate(n)}
          />
        </section>
      ) : (
        <GuessSection
          game={game}
          question={question}
          myId={myId}
          guessedUserIds={viewer.guessedUserIds}
          submitting={guessMutation.isPending}
          onGuess={(choiceUserId, notations) => guessMutation.mutate({ choiceUserId, notations })}
        />
      )}
    </div>
  );
}

function GuessSection({
  game,
  question,
  myId,
  guessedUserIds,
  submitting,
  onGuess,
}: {
  game: Game;
  question: Question;
  myId: string | undefined;
  guessedUserIds: string[];
  submitting: boolean;
  onGuess: (choiceUserId: string, notations: string[]) => void;
}) {
  const { t } = useTranslation();
  const remaining = game.players.filter((p) => p.id !== myId && !guessedUserIds.includes(p.id));

  if (remaining.length === 0) {
    const waitingFor = game.awaitingPlayerIds
      .filter((id) => id !== myId)
      .map((id) => game.players.find((p) => p.id === id)?.userName ?? id.slice(0, 8));

    return (
      <section className="card waiting">
        <h3>{t("play.waitingOthers")}</h3>
        {waitingFor.length > 0 && (
          <p className="waiting-names">{t("play.waitingFor", { names: waitingFor.join(", ") })}</p>
        )}
      </section>
    );
  }

  return (
    <>
      <p className="muted">{t("play.guessIntro")}</p>
      {remaining.map((p) => (
        <section className="card" key={p.id}>
          <h3>{t("play.guessFor", { name: p.userName })}</h3>
          <AnswerForm
            question={question}
            submitLabel={t("play.guess")}
            submitting={submitting}
            onSubmit={(n) => onGuess(p.id, n)}
          />
        </section>
      ))}
    </>
  );
}
