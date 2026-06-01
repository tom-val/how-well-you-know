import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import { addQuestion, deleteQuestion, startGame } from "../../api/gamesApi";
import type { Game } from "../../api/gamesApi";
import { useLang } from "../../i18n/lang";
import { useAuth } from "../../hooks/useAuth";
import { Avatar, Icon, ScreenHead, StatusBadge } from "../../components/ui";
import { playerColor } from "../../components/ui";
import { QuestionModal } from "./QuestionModal";
import type { NewQuestion } from "./QuestionModal";

const ALPHA = ["A", "B", "C", "D", "E", "F"];

export function RoomView({ game }: { game: Game }) {
  const { t } = useLang();
  const { user } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [modal, setModal] = useState(false);
  const [copied, setCopied] = useState(false);
  const link = `${window.location.origin}/join/${game.id}`;

  const refresh = () => queryClient.invalidateQueries({ queryKey: ["game", game.id] });
  const onErr = (err: unknown) => enqueueSnackbar(err instanceof Error ? err.message : t.error, { variant: "error" });

  const addMutation = useMutation({
    mutationFn: (q: NewQuestion) => {
      const map: Record<string, string> = {};
      q.options.forEach((o, i) => { map[ALPHA[i]] = o; });
      return addQuestion(game.id, q.text, q.multi, map);
    },
    onSuccess: refresh,
    onError: onErr,
  });
  const delMutation = useMutation({
    mutationFn: (qid: string) => deleteQuestion(game.id, qid),
    onSuccess: refresh,
    onError: onErr,
  });
  const startMutation = useMutation({
    mutationFn: () => startGame(game.id),
    onSuccess: () => { refresh(); navigate(`/games/${game.id}/play`); },
    onError: onErr,
  });

  const copy = () => {
    navigator.clipboard?.writeText(link).catch(() => {});
    setCopied(true);
    setTimeout(() => setCopied(false), 1800);
  };

  const canStart = game.players.length >= 2 && game.questions.length >= 2;

  return (
    <div className="page">
      <ScreenHead onBack={() => navigate("/")} title={game.name} t={t} right={<StatusBadge status="draft" t={t} />} />

      <div className="room-grid">
        <div className="stack" style={{ gap: 18 }}>
          <div className="card invite-card enter">
            <div className="row" style={{ gap: 9, marginBottom: 2 }}>
              <span style={{ color: "var(--primary)", display: "flex" }}><Icon.link /></span>
              <h3 style={{ fontSize: 17 }}>{t.inviteTitle}</h3>
            </div>
            <div className="mh-sub" style={{ fontSize: 13.5, color: "var(--ink-faint)" }}>{t.inviteSub}</div>
            <button className="btn btn-soft btn-block invite-btn" onClick={copy}>
              {copied ? <><Icon.check />{t.copied}</> : <><Icon.copy />{t.copyLink}</>}
            </button>
          </div>

          <div className="card card-pad enter enter-d1">
            <div className="between" style={{ marginBottom: 14 }}>
              <h3 className="section-title" style={{ fontSize: 21 }}>
                {t.questions} <span className="muted" style={{ fontWeight: 700 }}>{game.questions.length}</span>
              </h3>
            </div>

            {game.questions.length === 0 ? (
              <div className="empty" style={{ padding: "30px 20px" }}>
                <h3 style={{ fontSize: 16 }}>{t.noQuestions}</h3>
                <p>{t.noQuestionsSub}</p>
              </div>
            ) : (
              <div className="q-list">
                {game.questions.map((q, i) => (
                  <div className="q-item" key={q.id}>
                    <div className="q-num">{i + 1}</div>
                    <div className="grow">
                      <div className="q-text">{q.text}</div>
                      <div className="q-opts">
                        {q.variants.map((o, j) => <span className="q-opt" key={o.id}>{ALPHA[j]} · {o.text}</span>)}
                        {q.multipleAnswers && <span className="badge tag" style={{ padding: "3px 9px", fontSize: 11 }}>{t.multiTag}</span>}
                      </div>
                    </div>
                    <button className="q-del" onClick={() => delMutation.mutate(q.id)} aria-label={t.deleteQ}><Icon.trash /></button>
                  </div>
                ))}
              </div>
            )}

            <button className="add-q-tile" onClick={() => setModal(true)}>
              <Icon.plus />{t.addQuestion}
            </button>
          </div>
        </div>

        <div className="stack" style={{ gap: 18 }}>
          <div className="card players-card enter enter-d1">
            <div className="between" style={{ marginBottom: 6 }}>
              <h3 style={{ fontSize: 17 }}>{t.players}</h3>
              <span className="chip" style={{ padding: "3px 10px" }}>{game.players.length}</span>
            </div>
            {game.players.map((p) => (
              <div className="player-row" key={p.id}>
                <Avatar name={p.userName} color={playerColor(p.id)} size={34} />
                <span className="pr-name grow">{p.id === user?.sub ? t.you : p.userName}</span>
                {p.id === game.createdByUser && <span className="host-pill">{t.createdByYou}</span>}
              </div>
            ))}
          </div>

          <div className="enter enter-d2">
            <button className="btn btn-primary btn-lg btn-block" disabled={!canStart || startMutation.isPending} onClick={() => startMutation.mutate()}>
              <Icon.spark />{t.startGame}
            </button>
            {!canStart && <div className="start-note">{t.needMore}</div>}
          </div>
        </div>
      </div>

      <QuestionModal
        open={modal}
        onClose={() => setModal(false)}
        onSave={(q) => addMutation.mutate(q)}
        existing={game.questions.map((q) => q.text)}
      />
    </div>
  );
}
