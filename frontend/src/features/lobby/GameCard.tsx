import type { GameSummary } from "../../api/gamesApi";
import type { T } from "../../i18n/strings";
import { Icon, StatusBadge } from "../../components/ui";
import { uiStatus } from "../game/mappers";

const GC_COLORS: Record<string, string> = {
  live: "var(--c-amber)",
  draft: "var(--c-violet)",
  done: "var(--c-mint)",
};

export function GameCard({ g, t, myId, onOpen, delay }: { g: GameSummary; t: T; myId?: string; onOpen: (id: string) => void; delay?: string }) {
  const status = uiStatus(g.status);
  const hosting = g.createdByUser === myId;
  return (
    <button className={`game-card enter ${delay || ""}`} onClick={() => onOpen(g.gameId)}>
      <div
        className="gc-icon"
        style={{ background: `linear-gradient(140deg, ${GC_COLORS[status]}, color-mix(in oklch, ${GC_COLORS[status]}, #000 16%))` }}
      >
        <Icon.spark />
      </div>
      <div className="gc-main">
        <div className="gc-name">{g.name}</div>
        <div className="gc-meta">
          {hosting ? (
            <span className="host-tag"><Icon.trophy width="13" height="13" />{t.createdByYou}</span>
          ) : (
            <span className="muted" style={{ fontSize: 12.5, fontWeight: 600 }}>{t.joined}</span>
          )}
          <span className="dotsep"></span>
          <span className="muted" style={{ fontSize: 12.5, fontWeight: 600 }}>{t.qCount(g.questionCount ?? 0)}</span>
          <span className="dotsep"></span>
          <span className="muted" style={{ fontSize: 12.5, fontWeight: 600 }}>{t.pCount(g.playerCount ?? 0)}</span>
        </div>
      </div>
      <div className="gc-right">
        <StatusBadge status={status} t={t} />
        <span className="muted" style={{ display: "flex" }}><Icon.arrowR /></span>
      </div>
    </button>
  );
}
