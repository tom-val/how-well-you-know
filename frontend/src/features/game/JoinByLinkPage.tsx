import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { getGame, joinGame } from "../../api/gamesApi";
import { registerMe } from "../../api/usersApi";
import { useAuth } from "../../hooks/useAuth";
import { useLang } from "../../i18n/lang";
import { Icon, Spinner } from "../../components/ui";

/**
 * Opened from a share link (/join/:id). Ensures the user is registered, joins the game,
 * then redirects into it. If they're already a player (e.g. the host opening their own
 * link), it just sends them through.
 */
export default function JoinByLinkPage() {
  const { id = "" } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const { t } = useLang();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function run() {
      try {
        if (user) await registerMe(user.displayName);
        await joinGame(id);
        if (!cancelled) navigate(`/games/${id}`, { replace: true });
      } catch (err) {
        try {
          const game = await getGame(id);
          if (!cancelled && user && game.players.some((p) => p.id === user.sub)) {
            navigate(`/games/${id}`, { replace: true });
            return;
          }
        } catch {
          /* fall through */
        }
        if (!cancelled) setError(err instanceof Error ? err.message : t.error);
      }
    }
    run();
    return () => { cancelled = true; };
  }, [id, user, navigate, t]);

  if (error) {
    return (
      <div className="page" style={{ maxWidth: 480 }}>
        <div className="card card-pad enter">
          <h2 style={{ fontSize: 22, marginBottom: 8 }}>{t.error}</h2>
          <p className="muted" style={{ marginTop: 0 }}>{error}</p>
          <button className="lnk" onClick={() => navigate("/")}><Icon.arrowL />{t.backToGames}</button>
        </div>
      </div>
    );
  }

  return <Spinner t={t} />;
}
