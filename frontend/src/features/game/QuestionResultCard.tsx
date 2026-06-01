import { useTranslation } from "react-i18next";
import type { QuestionResult, VariantRef } from "../../api/gamesApi";

const fmt = (variants: VariantRef[]) => variants.map((v) => v.text).join(", ") || "—";

/**
 * Reveals a finished question: each player's actual pick, and what everyone guessed
 * they'd pick (with correct/incorrect). Used in the per-question review and final review.
 */
export function QuestionResultCard({
  result,
  nameOf,
}: {
  result: QuestionResult;
  nameOf: (userId: string) => string;
}) {
  const { t } = useTranslation();

  // Group every guess by the player it was about.
  const guessesAbout = new Map<string, { guesserId: string; guessed: VariantRef[]; score: number }[]>();
  for (const player of result.players) {
    for (const g of player.guesses) {
      const list = guessesAbout.get(g.choiceUserId) ?? [];
      list.push({ guesserId: player.userId, guessed: g.guessed, score: g.score });
      guessesAbout.set(g.choiceUserId, list);
    }
  }

  return (
    <section className="card">
      <h3 className="question-text">{result.text}</h3>
      <ul className="reveal-list">
        {result.answers.map((answer) => (
          <li key={answer.userId} className="reveal-subject">
            <div className="reveal-answer">
              <strong>{nameOf(answer.userId)}</strong>
              <span className="picked">{t("play.picked", { answer: fmt(answer.picked) })}</span>
            </div>
            <ul className="guess-list">
              {(guessesAbout.get(answer.userId) ?? []).map((g) => (
                <li key={g.guesserId}>
                  <span>{t("play.guesserGuessed", { name: nameOf(g.guesserId), answer: fmt(g.guessed) })}</span>
                  <span className={g.score > 0 ? "correct" : "wrong"}>{g.score > 0 ? "✓" : "✗"}</span>
                </li>
              ))}
            </ul>
          </li>
        ))}
      </ul>
    </section>
  );
}
