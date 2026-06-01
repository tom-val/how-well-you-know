import { useEffect, useRef } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getGame } from "../../api/gamesApi";
import { useGameSocket } from "../../api/useGameSocket";
import { useLang } from "../../i18n/lang";
import { Icon, Spinner } from "../../components/ui";
import { RoomView } from "./RoomView";
import { ActiveView } from "./ActiveView";
import { FinishedView } from "./FinishedView";

export default function GamePage() {
  const { t } = useLang();
  const { id = "" } = useParams();
  const navigate = useNavigate();

  // Real-time updates arrive over the socket; the interval is just a slow safety net.
  useGameSocket(id);
  const gameQuery = useQuery({
    queryKey: ["game", id],
    queryFn: () => getGame(id),
    refetchInterval: (query) => (query.state.data?.status === "Ended" ? false : 30000),
  });

  // If the game starts while we're sitting in the room, drop straight into play.
  // (Opening an already-live game from the lobby keeps showing the overview + Resume.)
  const prevStatus = useRef<string | undefined>(undefined);
  useEffect(() => {
    const status = gameQuery.data?.status;
    if (prevStatus.current === "Created" && status === "Started") {
      navigate(`/games/${id}/play`, { replace: true });
    }
    prevStatus.current = status;
  }, [gameQuery.data?.status, id, navigate]);

  if (gameQuery.isLoading) return <div className="page"><Spinner t={t} /></div>;
  if (gameQuery.isError || !gameQuery.data) {
    return (
      <div className="page">
        <p className="error-text muted">{t.error}</p>
        <button className="lnk" onClick={() => navigate("/")}><Icon.arrowL />{t.back}</button>
      </div>
    );
  }

  const game = gameQuery.data;
  if (game.status === "Started") return <ActiveView game={game} />;
  if (game.status === "Ended") return <FinishedView game={game} />;
  return <RoomView game={game} />;
}
