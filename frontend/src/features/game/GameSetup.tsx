import { useState } from "react";
import type { FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { addQuestion, startGame } from "../../api/gamesApi";
import type { Game } from "../../api/gamesApi";

const NOTATIONS = ["A", "B", "C", "D", "E", "F"];

export function GameSetup({ game }: { game: Game }) {
  const { t } = useTranslation();
  const { enqueueSnackbar } = useSnackbar();
  const queryClient = useQueryClient();

  const [text, setText] = useState("");
  const [multiple, setMultiple] = useState(false);
  const [variants, setVariants] = useState<string[]>(["", ""]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["game", game.id] });
  const onError = (err: unknown) =>
    enqueueSnackbar(err instanceof Error ? err.message : t("common.error"), { variant: "error" });

  const addQuestionMutation = useMutation({
    mutationFn: () => {
      const map: Record<string, string> = {};
      variants.forEach((v, i) => {
        if (v.trim()) map[NOTATIONS[i]] = v.trim();
      });
      return addQuestion(game.id, text.trim(), multiple, map);
    },
    onSuccess: () => {
      setText("");
      setMultiple(false);
      setVariants(["", ""]);
      invalidate();
    },
    onError,
  });

  const startMutation = useMutation({
    mutationFn: () => startGame(game.id),
    onSuccess: invalidate,
    onError,
  });

  function onAddQuestion(e: FormEvent) {
    e.preventDefault();
    const filled = variants.filter((v) => v.trim()).length;
    if (text.trim() && filled >= 2) addQuestionMutation.mutate();
  }

  const canStart = game.players.length >= 2 && game.questions.length >= 2;
  const shareUrl = `${window.location.origin}/join/${game.id}`;

  async function copyShareLink() {
    try {
      await navigator.clipboard.writeText(shareUrl);
      enqueueSnackbar(t("play.linkCopied"), { variant: "success" });
    } catch {
      enqueueSnackbar(shareUrl, { variant: "info" });
    }
  }

  return (
    <div className="page">
      <section className="card">
        <p className="muted">{t("play.shareHint")}</p>
        <div className="share-row">
          <code className="game-id">{shareUrl}</code>
          <button type="button" className="primary" onClick={copyShareLink}>
            {t("play.copyLink")}
          </button>
        </div>
      </section>

      <section className="card">
        <h3>
          {t("game.questions")} ({game.questions.length})
        </h3>
        {game.questions.length === 0 ? (
          <p className="muted">{t("play.noQuestionsYet")}</p>
        ) : (
          <ul className="question-list">
            {game.questions.map((q) => (
              <li key={q.id}>
                {q.text}
                <span className="muted"> · {q.variants.length}</span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="card">
        <h3>{t("play.addQuestion")}</h3>
        <form onSubmit={onAddQuestion} className="form">
          <label>
            {t("play.questionText")}
            <input value={text} onChange={(e) => setText(e.target.value)} required />
          </label>

          {variants.map((v, i) => (
            <label key={i}>
              {t("play.variant", { notation: NOTATIONS[i] })}
              <input
                value={v}
                onChange={(e) =>
                  setVariants((cur) => cur.map((x, j) => (j === i ? e.target.value : x)))
                }
              />
            </label>
          ))}

          <div className="setup-actions">
            {variants.length < NOTATIONS.length && (
              <button type="button" className="link-btn" onClick={() => setVariants((c) => [...c, ""])}>
                + {t("play.addVariant")}
              </button>
            )}
            <label className="inline-check">
              <input type="checkbox" checked={multiple} onChange={(e) => setMultiple(e.target.checked)} />
              {t("play.multipleAnswers")}
            </label>
          </div>

          <button type="submit" className="primary" disabled={addQuestionMutation.isPending}>
            {t("play.saveQuestion")}
          </button>
        </form>
      </section>

      <section className="card">
        <button
          type="button"
          className="primary block"
          disabled={!canStart || startMutation.isPending}
          onClick={() => startMutation.mutate()}
        >
          {t("play.start")}
        </button>
        {!canStart && <p className="muted">{t("play.startHint")}</p>}
      </section>
    </div>
  );
}
