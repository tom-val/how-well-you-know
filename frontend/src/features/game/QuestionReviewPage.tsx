import { useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getGame, getResults } from "../../api/gamesApi";
import { useLang } from "../../i18n/lang";
import { useAuth } from "../../hooks/useAuth";
import { Icon, ScreenHead, Spinner } from "../../components/ui";
import { QuestionReveal } from "./QuestionReveal";

export default function QuestionReviewPage() {
  const { t } = useLang();
  const { user } = useAuth();
  const navigate = useNavigate();
  const { id = "", qi = "0" } = useParams();
  const index = Number(qi) || 0;

  const gameQuery = useQuery({ queryKey: ["game", id], queryFn: () => getGame(id) });
  const resultsQuery = useQuery({ queryKey: ["results", id], queryFn: () => getResults(id) });

  if (gameQuery.isLoading || resultsQuery.isLoading || !gameQuery.data || !resultsQuery.data) {
    return <div className="page"><Spinner t={t} /></div>;
  }

  const game = gameQuery.data;
  const total = resultsQuery.data.questions.length;
  const result = resultsQuery.data.questions[index];
  if (!result) {
    return <div className="page"><p className="muted">{t.notFound}</p></div>;
  }

  return (
    <div className="page">
      <ScreenHead onBack={() => navigate(`/games/${id}`)} title={t.qReviewTag} t={t} right={<span className="chip">{index + 1} / {total}</span>} />

      <div className="review-wrap">
        <div className="q-hero enter" style={{ marginBottom: 18 }}>
          <div className="qh-eyebrow">
            <span className="badge tag"><Icon.spark width="13" height="13" />{t.questionN(index + 1)}</span>
          </div>
          <div className="qh-text" style={{ fontSize: "clamp(22px,3.4vw,30px)" }}>{result.text}</div>
        </div>

        <div className="eyebrow enter enter-d1" style={{ marginBottom: 12 }}>{t.everyonePicked}</div>

        <QuestionReveal result={result} players={game.players} meId={user?.sub} t={t} delayBase={0.1} />

        <div className="qr-nav enter" style={{ animationDelay: "0.3s" }}>
          <button className="btn btn-ghost" disabled={index === 0} onClick={() => navigate(`/games/${id}/q/${index - 1}`)}>
            <Icon.arrowL width="16" height="16" />{t.prevQuestion}
          </button>
          <button className="btn btn-ghost" disabled={index >= total - 1} onClick={() => navigate(`/games/${id}/q/${index + 1}`)}>
            {t.nextQ}<Icon.arrowR width="16" height="16" />
          </button>
        </div>
      </div>
    </div>
  );
}
