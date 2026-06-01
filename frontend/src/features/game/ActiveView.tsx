import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getResults } from "../../api/gamesApi";
import type { Game } from "../../api/gamesApi";
import { useLang } from "../../i18n/lang";
import { useAuth } from "../../hooks/useAuth";
import { Avatar, Icon, ScreenHead, StatusBadge, playerColor } from "../../components/ui";

export function ActiveView({ game }: { game: Game }) {
  const { t } = useLang();
  const { user } = useAuth();
  const navigate = useNavigate();

  const resultsQuery = useQuery({ queryKey: ["results", game.id], queryFn: () => getResults(game.id), refetchInterval: 4000 });
  const scores = new Map((resultsQuery.data?.overall ?? []).map((s) => [s.userId, s.totalScore]));

  const nameOf = (id: string) => (id === user?.sub ? t.you : game.players.find((p) => p.id === id)?.userName ?? "?");
  const ranked = [...game.players].map((p) => ({ ...p, score: scores.get(p.id) ?? 0 })).sort((a, b) => b.score - a.score);

  const total = game.questions.length;
  const currentIndex = Math.max(0, game.questions.findIndex((q) => q.id === game.currentQuestionId));
  const currentQ = game.questions[currentIndex];
  const answeredQuestions = game.questions.filter((q) => q.answered).length;
  const pct = total ? Math.round((answeredQuestions / total) * 100) : 0;

  const awaiting = new Set(game.awaitingPlayerIds);
  const answeredN = game.players.filter((p) => !awaiting.has(p.id)).length;

  return (
    <div className="page">
      <ScreenHead onBack={() => navigate("/")} title={game.name} t={t} right={<StatusBadge status="live" t={t} />} />

      <div className="live-hero enter">
        <div className="between" style={{ marginBottom: 14 }}>
          <span className="badge live"><span className="dot"></span>{t.activeTag}</span>
          <span className="lh-round">{t.roundOf(currentIndex + 1, total)}</span>
        </div>
        <div className="lh-q">{currentQ ? currentQ.text : "—"}</div>
        <div className="lh-bar"><i style={{ width: `${pct}%` }}></i></div>
        <div className="lh-foot">
          <span className="chip"><Icon.spark width="13" height="13" />{t.answeredCount(answeredN, game.players.length)}</span>
        </div>
      </div>

      <div className="room-grid" style={{ marginTop: 18 }}>
        <div className="card card-pad enter enter-d1">
          <h3 className="section-title" style={{ fontSize: 20, marginBottom: 14 }}>{t.standings}</h3>
          <div className="board" style={{ marginBottom: 0 }}>
            {ranked.map((p, i) => (
              <div className={`board-row ${i === 0 ? "first" : ""}`} key={p.id}>
                <div className="board-rank">{i + 1}</div>
                <Avatar name={p.userName} color={playerColor(p.id)} size={36} ring={i === 0} />
                <div className="board-main">
                  <div className="board-name">{nameOf(p.id)}{i === 0 ? ` · ${t.leader}` : ""}</div>
                  <div className="board-sub">{t.correctReads(p.score)}</div>
                </div>
                <div className="board-pts">{p.score}<span>{t.points}</span></div>
              </div>
            ))}
          </div>
        </div>

        <div className="stack" style={{ gap: 18 }}>
          <div className="card card-pad enter enter-d1">
            <h3 style={{ fontSize: 17, marginBottom: 12 }}>{t.currentRound}</h3>
            {game.players.map((p) => {
              const ok = !awaiting.has(p.id);
              return (
                <div className="answer-row" key={p.id}>
                  <Avatar name={p.userName} color={playerColor(p.id)} size={32} />
                  <span className="ar-name grow">{nameOf(p.id)}</span>
                  <span className={`ar-state ${ok ? "ok" : "wait"}`}>
                    {ok ? <><Icon.check width="13" height="13" />{t.statusAnswered}</> : t.statusThinking}
                  </span>
                </div>
              );
            })}
          </div>
          <div className="enter enter-d2">
            <button className="btn btn-primary btn-lg btn-block" onClick={() => navigate(`/games/${game.id}/play`)}>
              <Icon.spark />{t.resume}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
