import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getGame } from "../../api/gamesApi";
import { useLang } from "../../i18n/lang";
import { Icon, Spinner } from "../../components/ui";
import { RoomView } from "./RoomView";
import { ActiveView } from "./ActiveView";
import { FinishedView } from "./FinishedView";

export default function GamePage() {
  const { t } = useLang();
  const { id = "" } = useParams();
  const navigate = useNavigate();

  const gameQuery = useQuery({
    queryKey: ["game", id],
    queryFn: () => getGame(id),
    refetchInterval: (query) => (query.state.data?.status === "Ended" ? false : 5000),
  });

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
