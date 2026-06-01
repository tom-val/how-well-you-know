import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { getGame } from "../../api/gamesApi";
import { Spinner } from "../../components/Spinner";

export default function GamePage() {
  const { t } = useTranslation();
  const { id = "" } = useParams();

  // Poll so players joining / state changes show up live (no WebSockets yet).
  const gameQuery = useQuery({
    queryKey: ["game", id],
    queryFn: () => getGame(id),
    refetchInterval: 4000,
  });

  if (gameQuery.isLoading) return <Spinner />;
  if (gameQuery.isError || !gameQuery.data) {
    return (
      <div className="page">
        <p className="error-text">{t("common.error")}</p>
        <Link to="/" className="link-btn">
          {t("game.back")}
        </Link>
      </div>
    );
  }

  const game = gameQuery.data;

  return (
    <div className="page game-view">
      <div className="game-head">
        <Link to="/" className="link-btn">
          ← {t("game.back")}
        </Link>
        <span className={`badge status-${game.status.toLowerCase()}`}>{t(`status.${game.status}`)}</span>
      </div>

      <h2>{game.name}</h2>
      <code className="game-id">{game.id}</code>

      <section className="card">
        <h3>
          {t("game.players")} ({game.players.length})
        </h3>
        <ul className="player-list">
          {game.players.map((p) => (
            <li key={p.id}>{p.userName}</li>
          ))}
        </ul>
      </section>

      <section className="card">
        <h3>
          {t("game.questions")} ({game.questions.length})
        </h3>
        <ul className="question-list">
          {game.questions.map((q) => (
            <li key={q.id}>{q.text}</li>
          ))}
        </ul>
      </section>

      <p className="muted">{t("game.playComingSoon")}</p>
    </div>
  );
}
