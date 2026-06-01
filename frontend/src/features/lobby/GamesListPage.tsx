import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { listMyGames } from "../../api/gamesApi";
import { useLang } from "../../i18n/lang";
import { useAuth } from "../../hooks/useAuth";
import { Icon, ScreenHead, Spinner } from "../../components/ui";
import { uiStatus } from "../game/mappers";
import { GameCard } from "./GameCard";

export default function GamesListPage() {
  const { t } = useLang();
  const { user } = useAuth();
  const navigate = useNavigate();
  const myId = user?.sub;

  const [filter, setFilter] = useState<"all" | "hosting" | "joined">("all");
  const [status, setStatus] = useState<"all" | "live" | "draft" | "done">("all");
  const [q, setQ] = useState("");

  const gamesQuery = useQuery({ queryKey: ["myGames"], queryFn: listMyGames });
  const games = gamesQuery.data ?? [];

  const filters: [typeof filter, string][] = [["all", t.filterAll], ["hosting", t.filterHosting], ["joined", t.filterJoined]];
  const statuses: [typeof status, string][] = [["all", t.filterAll], ["live", t.st_live], ["draft", t.st_draft], ["done", t.st_done]];

  const shown = games.filter((g) => {
    const hosting = g.createdByUser === myId;
    if (filter === "hosting" && !hosting) return false;
    if (filter === "joined" && hosting) return false;
    if (status !== "all" && uiStatus(g.status) !== status) return false;
    if (q.trim() && !g.name.toLowerCase().includes(q.trim().toLowerCase())) return false;
    return true;
  });

  return (
    <div className="page">
      <ScreenHead onBack={() => navigate("/")} title={t.allGames} t={t} />

      <div className="filter-bar enter">
        <div className="search-box">
          <span className="search-ic"><Icon.search width="17" height="17" /></span>
          <input className="input" style={{ paddingLeft: 40 }} placeholder={t.searchPh} value={q} onChange={(e) => setQ(e.target.value)} />
        </div>
        <div className="seg-row">
          <div className="filter-seg">
            {filters.map(([k, label]) => (
              <button key={k} className={`fseg ${filter === k ? "on" : ""}`} onClick={() => setFilter(k)}>{label}</button>
            ))}
          </div>
          <div className="filter-seg">
            {statuses.map(([k, label]) => (
              <button key={k} className={`fseg ${status === k ? "on" : ""}`} onClick={() => setStatus(k)}>{label}</button>
            ))}
          </div>
        </div>
      </div>

      {gamesQuery.isLoading ? (
        <Spinner t={t} />
      ) : shown.length === 0 ? (
        <div className="empty enter enter-d1" style={{ marginTop: 18 }}>
          <div className="e-emoji"><Icon.spark /></div>
          <h3>{t.noMatch}</h3>
          <p>{t.noMatchSub}</p>
        </div>
      ) : (
        <div className="games-grid" style={{ marginTop: 18 }}>
          {shown.map((g, i) => (
            <GameCard key={g.gameId} g={g} t={t} myId={myId} onOpen={(id) => navigate(`/games/${id}`)} delay={`enter-d${Math.min(i + 1, 3)}`} />
          ))}
        </div>
      )}
    </div>
  );
}
