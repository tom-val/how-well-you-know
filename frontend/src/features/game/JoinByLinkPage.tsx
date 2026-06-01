import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getGame, joinGame } from "../../api/gamesApi";
import { registerMe } from "../../api/usersApi";
import { useAuth } from "../../hooks/useAuth";

/**
 * Opened from a share link (/join/:id). Ensures the user is registered, joins the game,
 * then redirects into it. If they're already a player (e.g. the host opening their own
 * link), it just sends them through.
 */
export default function JoinByLinkPage() {
  const { id = "" } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function run() {
      try {
        if (user) await registerMe(user.displayName);
        await joinGame(id);
        if (!cancelled) navigate(`/games/${id}`, { replace: true });
      } catch (err) {
        // Joining can fail because we're already a member — in that case just go in.
        try {
          const game = await getGame(id);
          if (!cancelled && user && game.players.some((p) => p.id === user.sub)) {
            navigate(`/games/${id}`, { replace: true });
            return;
          }
        } catch {
          // fall through to showing the original error
        }
        if (!cancelled) setError(err instanceof Error ? err.message : t("common.error"));
      }
    }

    run();
    return () => {
      cancelled = true;
    };
  }, [id, user, navigate, t]);

  if (error) {
    return (
      <div className="page">
        <section className="card">
          <h2>{t("join.couldNotJoin")}</h2>
          <p className="error-text">{error}</p>
          <Link to="/" className="link-btn">
            ← {t("play.backToLobby")}
          </Link>
        </section>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="spinner">{t("join.joining")}</div>
    </div>
  );
}
