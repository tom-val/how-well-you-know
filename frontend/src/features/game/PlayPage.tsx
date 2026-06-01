import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { advanceQuestion, getGame, getResults, recordChoice, recordGuess } from "../../api/gamesApi";
import type { Game, Question } from "../../api/gamesApi";
import { useGameSocket } from "../../api/useGameSocket";
import { useLang } from "../../i18n/lang";
import { useAuth } from "../../hooks/useAuth";
import { Avatar, Icon, ScreenHead, Spinner, playerColor } from "../../components/ui";
import { QuestionReveal } from "./QuestionReveal";

export default function PlayPage() {
  const { t } = useLang();
  const { user } = useAuth();
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { enqueueSnackbar } = useSnackbar();

  // Real-time updates arrive over the socket; intervals are slow safety nets.
  useGameSocket(id);
  const gameQuery = useQuery({ queryKey: ["game", id], queryFn: () => getGame(id), refetchInterval: 30000 });
  const game = gameQuery.data;

  // live scores for the header scoreboard (results works mid-game)
  const scoresQuery = useQuery({ queryKey: ["results", id], queryFn: () => getResults(id), refetchInterval: 30000 });
  const scoreOf = (uid: string) => scoresQuery.data?.overall.find((s) => s.userId === uid)?.totalScore ?? 0;

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["game", id] });
  const onErr = (err: unknown) => enqueueSnackbar(err instanceof Error ? err.message : t.error, { variant: "error" });

  const choiceMutation = useMutation({ mutationFn: (n: string[]) => recordChoice(id, n), onSuccess: refresh, onError: onErr });
  const guessMutation = useMutation({ mutationFn: (v: { target: string; n: string[] }) => recordGuess(id, v.target, v.n), onSuccess: refresh, onError: onErr });
  const advanceMutation = useMutation({
    mutationFn: () => advanceQuestion(id),
    onSuccess: (g) => {
      queryClient.setQueryData(["game", id], g);
      queryClient.invalidateQueries({ queryKey: ["results", id] });
      if (g.status === "Ended") navigate(`/games/${id}/results`, { replace: true });
    },
    onError: onErr,
  });

  // leave play if the game isn't in progress
  useEffect(() => {
    if (!game) return;
    if (game.status === "Created") navigate(`/games/${id}`, { replace: true });
    if (game.status === "Ended") navigate(`/games/${id}/results`, { replace: true });
  }, [game, id, navigate]);

  if (gameQuery.isLoading || !game) return <div className="page"><Spinner t={t} /></div>;
  if (game.status !== "Started") return <div className="page"><Spinner t={t} /></div>;

  const current = game.questions.find((q) => q.id === game.currentQuestionId);
  if (!current) return <div className="page"><Spinner t={t} /></div>;

  const total = game.questions.length;
  const qIndex = game.questions.findIndex((q) => q.id === current.id);
  const others = game.players.filter((p) => p.id !== user?.sub);
  const guessed = new Set(game.viewer?.guessedUserIds ?? []);
  const remaining = others.filter((p) => !guessed.has(p.id));

  const progressBar = (
    <div className="progress">
      {game.questions.map((q, i) => <i key={q.id} className={q.answered ? "done" : i === qIndex ? "on" : ""}></i>)}
    </div>
  );
  const headRight = (
    <div className="score-strip">
      {[...game.players]
        .sort((a, b) => scoreOf(b.id) - scoreOf(a.id))
        .map((p) => (
          <span className={`score-pill ${p.id === user?.sub ? "me" : ""}`} key={p.id} title={p.userName}>
            <Avatar name={p.userName} color={playerColor(p.id)} size={20} />
            <span className="sp-name">{p.id === user?.sub ? t.you : p.userName}</span>
            <b className="sp-score">{scoreOf(p.id)}</b>
          </span>
        ))}
    </div>
  );

  // ---- REVIEW ----
  if (game.currentQuestionPhase === "Review") {
    const moreToCome = game.questions.some((q) => !q.answered && q.id !== current.id);
    return (
      <div className="page">
        <ScreenHead onBack={() => navigate(`/games/${id}`)} title={game.name} t={t} right={headRight} />
        <div className="review-wrap enter">
          {progressBar}
          <div className="q-hero" style={{ marginBottom: 18 }}>
            <div className="qh-eyebrow">
              <span className="badge tag"><Icon.spark width="13" height="13" />{t.reviewTag}</span>
            </div>
            <div className="qh-text" style={{ fontSize: "clamp(22px,3.4vw,30px)" }}>{current.text}</div>
          </div>
          <ReviewBody gameId={id} questionId={current.id} game={game} meId={user?.sub} t={t} />
          <button className="btn btn-primary btn-lg btn-block enter" style={{ marginTop: 8 }} disabled={advanceMutation.isPending} onClick={() => advanceMutation.mutate()}>
            {moreToCome ? <>{t.nextQuestion}<Icon.arrowR /></> : <><Icon.trophy />{t.finishGame}</>}
          </button>
        </div>
      </div>
    );
  }

  // ---- ANSWER ----
  if (!game.viewer?.hasAnswered) {
    return (
      <div className="page">
        <ScreenHead onBack={() => navigate(`/games/${id}`)} title={game.name} t={t} right={headRight} />
        <div className="play-wrap enter">
          {progressBar}
          <div className="q-hero">
            <div className="qh-eyebrow">
              <span className="badge live"><span className="dot"></span>{t.yourTurn}</span>
              <span className="muted" style={{ fontSize: 13, fontWeight: 700 }}>{t.questionOf(qIndex + 1, total)}</span>
            </div>
            <div className="qh-text">{current.text}</div>
          </div>
          <AnswerCard key={current.id} question={current} multi={current.multipleAnswers} submitting={choiceMutation.isPending} t={t} guess={false}
            onSubmit={(n) => choiceMutation.mutate(n)} />
        </div>
      </div>
    );
  }

  // ---- GUESS ----
  if (remaining.length > 0) {
    const target = remaining[0];
    const gi = others.length - remaining.length;
    return (
      <div className="page">
        <ScreenHead onBack={() => navigate(`/games/${id}`)} title={game.name} t={t} right={headRight} />
        <div className="play-wrap enter" key={target.id}>
          {progressBar}
          <div className="q-hero guess">
            <div className="qh-eyebrow">
              <span className="badge tag"><Icon.spark width="13" height="13" />{t.guessPhase}</span>
              <span className="muted" style={{ fontSize: 13, fontWeight: 700 }}>{t.guessOf(gi + 1, others.length)}</span>
            </div>
            <div className="qh-text" style={{ fontSize: "clamp(20px,3vw,26px)", marginBottom: 2 }}>{current.text}</div>
          </div>
          <div className="guess-who">
            <Avatar name={target.userName} color={playerColor(target.id)} size={44} />
            <div className="gw-text">
              <div className="gw-label">{t.guessFor}</div>
              <div className="gw-name">{t.guessTitle(target.userName)}</div>
            </div>
          </div>
          <AnswerCard key={`${current.id}-${target.id}`} question={current} multi={current.multipleAnswers} submitting={guessMutation.isPending} t={t} guess
            lastTarget={remaining.length === 1}
            onSubmit={(n) => guessMutation.mutate({ target: target.id, n })} />
        </div>
      </div>
    );
  }

  // ---- WAITING ----
  const awaiting = new Set(game.awaitingPlayerIds);
  return (
    <div className="page">
      <ScreenHead onBack={() => navigate(`/games/${id}`)} title={game.name} t={t} right={headRight} />
      <div className="play-wrap enter">
        {progressBar}
        <div className="waiting-card">
          <div className="wait-spinner"><i></i></div>
          <h3>{t.waitingTitle}</h3>
          <p>{t.waitingSub}</p>
          <div className="wait-players">
            {game.players.map((p) => {
              const ready = !awaiting.has(p.id);
              return (
                <div className={`wait-p ${ready ? "ready" : "pending"}`} key={p.id}>
                  <Avatar name={p.userName} color={playerColor(p.id)} size={40} />
                  <span className="wp-name">{p.id === user?.sub ? t.you : p.userName}</span>
                  <span className="wp-state">{ready ? `✓ ${t.answered}` : t.statusThinking}</span>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}

/* ---- answer/guess option picker (own selection state) ---- */
function AnswerCard({ question, multi, submitting, t, guess, lastTarget, onSubmit }: {
  question: Question; multi: boolean; submitting: boolean; t: ReturnType<typeof useLang>["t"]; guess: boolean; lastTarget?: boolean; onSubmit: (n: string[]) => void;
}) {
  const [sel, setSel] = useState<string[]>([]);
  const toggle = (n: string) => {
    if (multi) setSel((s) => (s.includes(n) ? s.filter((x) => x !== n) : [...s, n]));
    else setSel([n]);
  };
  const variants = [...question.variants].sort((a, b) => a.notation.localeCompare(b.notation));

  return (
    <div className="card card-pad">
      {!guess && (
        <div style={{ marginBottom: 14 }}>
          <h3 style={{ fontSize: 17 }}>{t.yourAnswer}</h3>
          <div className="muted" style={{ fontSize: 13.5, marginTop: 3, fontWeight: 600 }}>{t.yourAnswerSub}</div>
        </div>
      )}
      {guess && <div className="muted" style={{ fontSize: 13.5, marginBottom: 12, fontWeight: 600 }}>{t.guessSub}</div>}
      {multi && (
        <span className="badge tag" style={{ marginBottom: 12, padding: "5px 11px" }}>
          <Icon.check width="13" height="13" />{t.multiHint}
        </span>
      )}
      <div className="opt-list">
        {variants.map((v) => {
          const on = sel.includes(v.notation);
          return (
            <button key={v.id} className={`opt-choice ${on ? "sel" : ""}`} onClick={() => toggle(v.notation)}>
              <span className="oc-key">{v.notation}</span>
              <span className="oc-label">{v.text}</span>
              <span className={`oc-radio ${multi ? "sq" : ""}`}></span>
            </button>
          );
        })}
      </div>
      <button className="btn btn-primary btn-lg btn-block play-cta" disabled={sel.length === 0 || submitting} onClick={() => onSubmit(sel)}>
        {sel.length === 0
          ? t.pickOne
          : guess
            ? (lastTarget ? <>{t.submitGuess}<Icon.check /></> : <>{t.next}<Icon.arrowR /></>)
            : <>{t.answer}<Icon.arrowR /></>}
      </button>
    </div>
  );
}

/* ---- review body: fetch results, reveal current question ---- */
function ReviewBody({ gameId, questionId, game, meId, t }: { gameId: string; questionId: string; game: Game; meId?: string; t: ReturnType<typeof useLang>["t"] }) {
  // Key by question + always refetch on mount: when a new question enters review we must
  // fetch fresh results (the shared cache may predate this question being answered).
  // Keep polling until the just-answered question shows up in the results.
  const resultsQuery = useQuery({
    queryKey: ["results", gameId, questionId],
    queryFn: () => getResults(gameId),
    staleTime: 0,
    refetchOnMount: "always",
    refetchInterval: (query) =>
      query.state.data?.questions.some((q) => q.questionId === questionId) ? false : 2000,
  });

  const result = resultsQuery.data?.questions.find((q) => q.questionId === questionId);
  if (!result) return <Spinner t={t} />;
  return <QuestionReveal result={result} players={game.players} meId={meId} t={t} />;
}
