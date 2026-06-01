import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { createGame, joinGame, listMyGames } from "../../api/gamesApi";
import { useLang } from "../../i18n/lang";
import { useAuth } from "../../hooks/useAuth";
import { Icon, Spinner } from "../../components/ui";
import { GameCard } from "./GameCard";

const PREVIEW = 3;

export default function LobbyPage() {
  const { t } = useLang();
  const { user } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [name, setName] = useState("");
  const [joinId, setJoinId] = useState("");

  const gamesQuery = useQuery({ queryKey: ["myGames"], queryFn: listMyGames });

  const onErr = (err: unknown) =>
    enqueueSnackbar(err instanceof Error ? err.message : t.error, { variant: "error" });
  const refresh = () => queryClient.invalidateQueries({ queryKey: ["myGames"] });

  const createMutation = useMutation({
    mutationFn: (gameName: string) => createGame(gameName),
    onSuccess: (g) => { refresh(); navigate(`/games/${g.id}`); },
    onError: onErr,
  });
  const joinMutation = useMutation({
    mutationFn: (id: string) => joinGame(id.trim()),
    onSuccess: (g) => { refresh(); navigate(`/games/${g.id}`); },
    onError: onErr,
  });

  const games = gamesQuery.data ?? [];
  const preview = games.slice(0, PREVIEW);

  return (
    <div className="page">
      <div className="lobby-hero enter">
        <div className="eyebrow" style={{ marginBottom: 8 }}>{t.brand}</div>
        <h1 className="screen-title">{t.yourGames}</h1>
      </div>

      <div className="action-row" style={{ marginTop: 22 }}>
        <div className="card action-card enter">
          <h3>{t.createGame}</h3>
          <div className="ac-sub">{t.createGameSub}</div>
          <form className="action-form" onSubmit={(e: FormEvent) => { e.preventDefault(); if (name.trim()) createMutation.mutate(name.trim()); }}>
            <input className="input" placeholder={t.gameNamePh} value={name} onChange={(e) => setName(e.target.value)} />
            <button className="btn btn-primary" type="submit" disabled={!name.trim() || createMutation.isPending}>{t.create}</button>
          </form>
        </div>
        <div className="card action-card enter enter-d1">
          <h3>{t.joinGame}</h3>
          <div className="ac-sub">{t.joinGameSub}</div>
          <form className="action-form" onSubmit={(e: FormEvent) => { e.preventDefault(); if (joinId.trim()) joinMutation.mutate(joinId.trim()); }}>
            <input className="input" placeholder={t.gameIdPh} value={joinId} onChange={(e) => setJoinId(e.target.value)} />
            <button className="btn btn-ghost" type="submit" disabled={!joinId.trim() || joinMutation.isPending}>{t.join}</button>
          </form>
        </div>
      </div>

      <div className="list-head enter enter-d2" style={{ marginTop: 34 }}>
        <div>
          <h2 className="section-title">{t.recentGames}</h2>
          <div className="muted" style={{ fontSize: 13.5, fontWeight: 600, marginTop: 2 }}>{t.recentGamesSub}</div>
        </div>
        {games.length > PREVIEW && (
          <button className="lnk" onClick={() => navigate("/games")}>{t.seeAll} ({games.length})<Icon.arrowR width="15" height="15" /></button>
        )}
      </div>

      {gamesQuery.isLoading ? (
        <Spinner t={t} />
      ) : games.length === 0 ? (
        <div className="empty enter enter-d2">
          <div className="e-emoji"><Icon.spark /></div>
          <h3>{t.noGames}</h3>
          <p>{t.noGamesSub}</p>
        </div>
      ) : (
        <div className="games-grid" style={{ marginTop: 14 }}>
          {preview.map((g, i) => (
            <GameCard key={g.gameId} g={g} t={t} myId={user?.sub} onOpen={(id) => navigate(`/games/${id}`)} delay={`enter-d${Math.min(i + 1, 3)}`} />
          ))}
        </div>
      )}
    </div>
  );
}
