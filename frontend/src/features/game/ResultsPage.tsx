import { useEffect, useRef } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { createGame, getGame, getResults } from "../../api/gamesApi";
import { useLang } from "../../i18n/lang";
import { useAuth } from "../../hooks/useAuth";
import { Avatar, Icon, Spinner, playerColor } from "../../components/ui";

const CONFETTI_VARS = ["--c-violet", "--c-coral", "--c-mint", "--c-amber", "--c-sky", "--c-pink"];

export default function ResultsPage() {
  const { t } = useLang();
  const { user } = useAuth();
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();

  const gameQuery = useQuery({ queryKey: ["game", id], queryFn: () => getGame(id) });
  const resultsQuery = useQuery({ queryKey: ["results", id], queryFn: () => getResults(id) });

  const playAgain = useMutation({
    mutationFn: () => createGame(gameQuery.data?.name ?? "Game"),
    onSuccess: (g) => navigate(`/games/${g.id}`),
    onError: (err) => enqueueSnackbar(err instanceof Error ? err.message : t.error, { variant: "error" }),
  });

  const confetti = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const host = confetti.current;
    if (!host) return;
    const css = getComputedStyle(document.documentElement);
    for (let i = 0; i < 70; i++) {
      const el = document.createElement("i");
      el.style.left = Math.random() * 100 + "%";
      el.style.background = css.getPropertyValue(CONFETTI_VARS[i % 6]) || "#7c5cff";
      el.style.animationDuration = 2.2 + Math.random() * 1.8 + "s";
      el.style.animationDelay = Math.random() * 0.6 + "s";
      el.style.opacity = "0.85";
      host.appendChild(el);
    }
    const tm = setTimeout(() => { if (host) host.innerHTML = ""; }, 4200);
    return () => { clearTimeout(tm); if (host) host.innerHTML = ""; };
  }, []);

  if (gameQuery.isLoading || resultsQuery.isLoading || !gameQuery.data || !resultsQuery.data) {
    return <div className="page"><Spinner t={t} /></div>;
  }
  const game = gameQuery.data;
  const nameOf = (uid: string) => (uid === user?.sub ? t.you : game.players.find((p) => p.id === uid)?.userName ?? "?");
  const ranked = [...resultsQuery.data.overall].sort((a, b) => a.rank - b.rank);
  const top = ranked[0];
  const tie = ranked.length > 1 && ranked[1].totalScore === top?.totalScore;

  return (
    <div className="page">
      <div className="confetti" ref={confetti}></div>
      <div className="results-wrap">
        <div className="trophy-badge enter"><Icon.trophy width="40" height="40" /></div>
        <div className="eyebrow enter enter-d1" style={{ marginBottom: 8 }}>{t.resultsTag}</div>
        <h1 className="results-title enter enter-d1">{tie ? t.tie : top ? (top.userId === user?.sub ? t.winnerYou : t.winnerIs(nameOf(top.userId))) : ""}</h1>
        <div className="results-sub enter enter-d2">{game.name}</div>

        <div className="board">
          {ranked.map((p, i) => (
            <div className={`board-row enter ${i === 0 ? "first" : ""}`} key={p.userId} style={{ animationDelay: `${0.15 + i * 0.07}s` }}>
              <div className="board-rank">{i + 1}</div>
              <Avatar name={nameOf(p.userId)} color={playerColor(p.userId)} size={38} ring={i === 0} />
              <div className="board-main">
                <div className="board-name">{nameOf(p.userId)}</div>
                <div className="board-sub">{t.correctReads(p.totalScore)}</div>
              </div>
              <div className="board-pts">{p.totalScore}<span>{t.points}</span></div>
            </div>
          ))}
        </div>

        <div className="results-actions enter" style={{ animationDelay: "0.5s" }}>
          <button className="btn btn-ghost btn-lg" onClick={() => navigate("/")}>{t.backToGames}</button>
          <button className="btn btn-primary btn-lg" onClick={() => playAgain.mutate()} disabled={playAgain.isPending}><Icon.spark />{t.playAgain}</button>
        </div>
      </div>
    </div>
  );
}
