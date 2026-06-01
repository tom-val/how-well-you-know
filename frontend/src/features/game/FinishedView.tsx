import { useNavigate } from "react-router-dom";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { createGame, getResults } from "../../api/gamesApi";
import type { Game } from "../../api/gamesApi";
import { useLang } from "../../i18n/lang";
import { useAuth } from "../../hooks/useAuth";
import { Avatar, Icon, ScreenHead, Spinner, StatusBadge, playerColor } from "../../components/ui";

export function FinishedView({ game }: { game: Game }) {
  const { t } = useLang();
  const { user } = useAuth();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const resultsQuery = useQuery({ queryKey: ["results", game.id], queryFn: () => getResults(game.id) });

  const playAgain = useMutation({
    mutationFn: () => createGame(game.name),
    onSuccess: (g) => navigate(`/games/${g.id}`),
    onError: (err) => enqueueSnackbar(err instanceof Error ? err.message : t.error, { variant: "error" }),
  });

  if (resultsQuery.isLoading || !resultsQuery.data) return <div className="page"><Spinner t={t} /></div>;
  const data = resultsQuery.data;

  const nameOf = (id: string) => (id === user?.sub ? t.you : game.players.find((p) => p.id === id)?.userName ?? "?");
  const ranked = [...data.overall].sort((a, b) => a.rank - b.rank);
  const top = ranked[0];
  const tie = ranked.length > 1 && ranked[1].totalScore === top?.totalScore;

  const qStats = data.questions.map((q) => {
    let correct = 0, total = 0;
    q.players.forEach((p) => p.guesses.forEach((g) => { total++; if (g.score > 0) correct++; }));
    return { correct, total };
  });

  return (
    <div className="page">
      <ScreenHead onBack={() => navigate("/")} title={game.name} t={t} right={<StatusBadge status="done" t={t} />} />

      <div className="winner-banner enter">
        <div className="wb-trophy"><Icon.trophy width="30" height="30" /></div>
        <div className="grow">
          <div className="eyebrow" style={{ color: "var(--c-amber)" }}>{t.finishedTag}</div>
          <div className="wb-title">{tie ? t.tie : top ? (top.userId === user?.sub ? t.winnerYou : t.winnerIs(nameOf(top.userId))) : ""}</div>
        </div>
      </div>

      <div className="finished-grid">
        <div className="card card-pad enter enter-d1">
          <h3 className="section-title" style={{ fontSize: 20, marginBottom: 14 }}>{t.finalStandings}</h3>
          <div className="board" style={{ marginBottom: 0 }}>
            {ranked.map((p, i) => (
              <div className={`board-row ${i === 0 ? "first" : ""}`} key={p.userId}>
                <div className="board-rank">{i + 1}</div>
                <Avatar name={nameOf(p.userId)} color={playerColor(p.userId)} size={36} ring={i === 0} />
                <div className="board-main">
                  <div className="board-name">{nameOf(p.userId)}</div>
                  <div className="board-sub">{t.correctReads(p.totalScore)}</div>
                </div>
                <div className="board-pts">{p.totalScore}<span>{t.points}</span></div>
              </div>
            ))}
          </div>
        </div>

        <div className="card card-pad enter enter-d2">
          <h3 className="section-title" style={{ fontSize: 20, marginBottom: 2 }}>{t.questionByQuestion}</h3>
          <div className="muted" style={{ fontSize: 13, fontWeight: 600, marginBottom: 14 }}>{t.qbqSub}</div>
          <div className="qbq-list">
            {data.questions.map((q, i) => (
              <button className="qbq-row" key={q.questionId} onClick={() => navigate(`/games/${game.id}/q/${i}`)}>
                <div className="qbq-num">{i + 1}</div>
                <div className="qbq-main">
                  <div className="qbq-text">{q.text}</div>
                  <div className="qbq-acc">{t.readAccuracy(qStats[i].correct, qStats[i].total)}</div>
                </div>
                <span className="qbq-go">{t.viewReview}<Icon.arrowR width="15" height="15" /></span>
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="enter enter-d3" style={{ marginTop: 20, display: "flex", gap: 12 }}>
        <button className="btn btn-ghost btn-lg grow" onClick={() => navigate("/")}>{t.backToGames}</button>
        <button className="btn btn-primary btn-lg grow" onClick={() => playAgain.mutate()} disabled={playAgain.isPending}>
          <Icon.spark />{t.playAgainSame}
        </button>
      </div>
    </div>
  );
}
