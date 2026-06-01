import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { getResults } from "../../api/gamesApi";
import type { Game } from "../../api/gamesApi";
import { Spinner } from "../../components/Spinner";
import { QuestionResultCard } from "./QuestionResultCard";

export function FinalResults({ game }: { game: Game }) {
  const { t } = useTranslation();
  const resultsQuery = useQuery({ queryKey: ["results", game.id, "final"], queryFn: () => getResults(game.id) });

  const nameOf = (userId: string) =>
    game.players.find((p) => p.id === userId)?.userName ?? userId.slice(0, 8);

  if (resultsQuery.isLoading) return <Spinner />;

  const overall = (resultsQuery.data?.overall ?? []).slice().sort((a, b) => a.rank - b.rank);
  const questions = resultsQuery.data?.questions ?? [];

  return (
    <div className="page">
      <section className="card">
        <span className="badge status-ended">{t("status.Ended")}</span>
        <h2>{t("play.results")}</h2>
        <ol className="leaderboard">
          {overall.map((s) => (
            <li key={s.userId}>
              <span className="rank">#{s.rank}</span>
              <span className="lb-name">{nameOf(s.userId)}</span>
              <span className="badge muted-badge">{t("play.points", { count: s.totalScore })}</span>
            </li>
          ))}
        </ol>
      </section>

      <h3 className="section-title">{t("play.review")}</h3>
      {questions.map((q) => (
        <QuestionResultCard key={q.questionId} result={q} nameOf={nameOf} />
      ))}

      <Link to="/" className="link-btn">
        ← {t("play.backToLobby")}
      </Link>
    </div>
  );
}
