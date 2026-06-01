import type { QuestionResult, Player, VariantRef } from "../../api/gamesApi";
import type { T } from "../../i18n/strings";
import { Avatar, Icon, playerColor } from "../../components/ui";

const fmt = (variants: VariantRef[]) => variants.map((v) => v.text).join(", ") || "—";

/** Renders one card per subject: their actual pick + how each other player guessed them. */
export function QuestionReveal({ result, players, meId, t, delayBase = 0 }: { result: QuestionResult; players: Player[]; meId?: string; t: T; delayBase?: number }) {
  const nameOf = (id: string) => (id === meId ? t.you : players.find((p) => p.id === id)?.userName ?? "?");
  const pickedBy = new Map(result.answers.map((a) => [a.userId, a.picked]));

  // subject -> guesses about them
  const guessesAbout = new Map<string, { guesserId: string; guessed: VariantRef[]; ok: boolean }[]>();
  for (const player of result.players) {
    for (const g of player.guesses) {
      const list = guessesAbout.get(g.choiceUserId) ?? [];
      list.push({ guesserId: player.userId, guessed: g.guessed, ok: g.score > 0 });
      guessesAbout.set(g.choiceUserId, list);
    }
  }

  // order subjects by the game's player order
  const subjects = players.filter((p) => pickedBy.has(p.id));

  return (
    <>
      {subjects.map((subject, si) => (
        <div className="card card-pad rev-player-card enter" key={subject.id} style={{ animationDelay: `${delayBase + si * 0.05}s` }}>
          <div className="row" style={{ gap: 13, marginBottom: 10 }}>
            <Avatar name={subject.userName} color={playerColor(subject.id)} size={42} />
            <div className="grow">
              <div className="rp-name" style={{ fontSize: 16 }}>{nameOf(subject.id)}</div>
              <div className="rp-line">{t.picked}: <b>{fmt(pickedBy.get(subject.id) ?? [])}</b></div>
            </div>
          </div>
          <div className="rev-guesses">
            {(guessesAbout.get(subject.id) ?? []).map((g) => (
              <div className="rev-guess" key={g.guesserId}>
                <span className={`gv-icon ${g.ok ? "ok" : "no"}`}>{g.ok ? <Icon.check width="13" height="13" /> : <Icon.x width="13" height="13" />}</span>
                {t.guessed(nameOf(g.guesserId))}: <b style={{ color: "var(--ink)" }}>{fmt(g.guessed)}</b>
              </div>
            ))}
          </div>
        </div>
      ))}
    </>
  );
}
