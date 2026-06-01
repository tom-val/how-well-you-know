import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { createGame, joinGame, listMyGames } from "../../api/gamesApi";
import { useAuth } from "../../hooks/useAuth";
import { Spinner } from "../../components/Spinner";

export default function LobbyPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [name, setName] = useState("");
  const [gameId, setGameId] = useState("");

  const gamesQuery = useQuery({ queryKey: ["myGames"], queryFn: listMyGames });

  const createMutation = useMutation({
    mutationFn: (gameName: string) => createGame(gameName),
    onSuccess: (game) => {
      queryClient.invalidateQueries({ queryKey: ["myGames"] });
      navigate(`/games/${game.id}`);
    },
    onError: (err) =>
      enqueueSnackbar(err instanceof Error ? err.message : t("common.error"), { variant: "error" }),
  });

  const joinMutation = useMutation({
    mutationFn: (id: string) => joinGame(id.trim()),
    onSuccess: (game) => {
      queryClient.invalidateQueries({ queryKey: ["myGames"] });
      navigate(`/games/${game.id}`);
    },
    onError: (err) =>
      enqueueSnackbar(err instanceof Error ? err.message : t("common.error"), { variant: "error" }),
  });

  function onCreate(e: FormEvent) {
    e.preventDefault();
    if (name.trim()) createMutation.mutate(name.trim());
  }

  function onJoin(e: FormEvent) {
    e.preventDefault();
    if (gameId.trim()) joinMutation.mutate(gameId.trim());
  }

  return (
    <div className="page lobby">
      <section className="card">
        <h2>{t("lobby.title")}</h2>
        {gamesQuery.isLoading ? (
          <Spinner />
        ) : gamesQuery.isError ? (
          <p className="error-text">{t("common.error")}</p>
        ) : gamesQuery.data && gamesQuery.data.length > 0 ? (
          <ul className="game-list">
            {gamesQuery.data.map((g) => (
              <li key={g.gameId}>
                <button type="button" className="game-item" onClick={() => navigate(`/games/${g.gameId}`)}>
                  <span className="game-item-name">{g.name}</span>
                  <span className={`badge status-${g.status.toLowerCase()}`}>{t(`status.${g.status}`)}</span>
                  {user && g.createdByUser === user.sub && (
                    <span className="badge muted-badge">{t("lobby.createdByYou")}</span>
                  )}
                </button>
              </li>
            ))}
          </ul>
        ) : (
          <p className="muted">{t("lobby.empty")}</p>
        )}
      </section>

      <div className="lobby-forms">
        <section className="card">
          <h3>{t("lobby.createTitle")}</h3>
          <form onSubmit={onCreate} className="form row">
            <input
              type="text"
              value={name}
              placeholder={t("lobby.nameLabel")}
              onChange={(e) => setName(e.target.value)}
              required
            />
            <button type="submit" className="primary" disabled={createMutation.isPending}>
              {t("lobby.create")}
            </button>
          </form>
        </section>

        <section className="card">
          <h3>{t("lobby.joinTitle")}</h3>
          <form onSubmit={onJoin} className="form row">
            <input
              type="text"
              value={gameId}
              placeholder={t("lobby.gameIdLabel")}
              onChange={(e) => setGameId(e.target.value)}
              required
            />
            <button type="submit" className="primary" disabled={joinMutation.isPending}>
              {t("lobby.join")}
            </button>
          </form>
        </section>
      </div>
    </div>
  );
}
