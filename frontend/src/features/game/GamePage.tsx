import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { getGame } from "../../api/gamesApi";
import { Spinner } from "../../components/Spinner";
import { GameSetup } from "./GameSetup";
import { AnswerPhase } from "./AnswerPhase";
import { ReviewPhase } from "./ReviewPhase";
import { FinalResults } from "./FinalResults";

export default function GamePage() {
  const { t } = useTranslation();
  const { id = "" } = useParams();

  // Poll so other players' moves / phase changes show up live (no WebSockets yet).
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
          ← {t("game.back")}
        </Link>
      </div>
    );
  }

  const game = gameQuery.data;
  const currentQuestion = game.questions.find((q) => q.id === game.currentQuestionId);

  return (
    <div className="game-view">
      <div className="game-head">
        <Link to="/" className="link-btn">
          ← {t("game.back")}
        </Link>
        <span className="players-count">
          {t("game.players")}: {game.players.length}
        </span>
      </div>
      <h1 className="game-title">{game.name}</h1>

      {game.status === "Created" && <GameSetup game={game} />}

      {game.status === "Started" &&
        (game.currentQuestionPhase === "Answering" && currentQuestion ? (
          <AnswerPhase game={game} question={currentQuestion} />
        ) : (
          <ReviewPhase game={game} />
        ))}

      {game.status === "Ended" && <FinalResults game={game} />}
    </div>
  );
}
