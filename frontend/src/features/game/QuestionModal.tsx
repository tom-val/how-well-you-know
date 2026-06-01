import { useEffect, useState } from "react";
import { useLang } from "../../i18n/lang";
import { Icon, Modal } from "../../components/ui";
import { suggestQuestion } from "./questionBank";
import { suggestAiQuestion } from "../../api/suggestionsApi";

const ALPHA = ["A", "B", "C", "D", "E", "F"];
const OPT_COLORS = ["var(--c-violet)", "var(--c-coral)", "var(--c-mint)", "var(--c-amber)", "var(--c-sky)", "var(--c-pink)"];

export interface NewQuestion {
  text: string;
  options: string[];
  multi: boolean;
}

export function QuestionModal({ open, onClose, onSave }: { open: boolean; onClose: () => void; onSave: (q: NewQuestion) => void }) {
  const { t, lang } = useLang();
  const [text, setText] = useState("");
  const [opts, setOpts] = useState<string[]>(["", ""]);
  const [multi, setMulti] = useState(false);
  const [aiBusy, setAiBusy] = useState(false);
  const [diceSpin, setDiceSpin] = useState(false);

  useEffect(() => {
    if (open) { setText(""); setOpts(["", ""]); setMulti(false); setAiBusy(false); }
  }, [open]);

  const valid = text.trim() !== "" && opts.filter((o) => o.trim()).length >= 2;
  const setOpt = (i: number, v: string) => setOpts((o) => o.map((x, j) => (j === i ? v : x)));
  const addOpt = () => setOpts((o) => (o.length < 6 ? [...o, ""] : o));
  const rmOpt = (i: number) => setOpts((o) => o.filter((_, j) => j !== i));

  const applyQuestion = (q: { text: string; options: string[] }) => {
    setText(q.text);
    const o = q.options.slice(0, 6);
    setOpts(o.length >= 2 ? o : [...o, "", ""].slice(0, 2));
    setMulti(false);
  };

  const suggestRandom = () => {
    setDiceSpin(true);
    setTimeout(() => setDiceSpin(false), 500);
    applyQuestion(suggestQuestion(lang));
  };

  const suggestAI = async () => {
    if (aiBusy) return;
    setAiBusy(true);
    try {
      applyQuestion(await suggestAiQuestion(lang));
    } catch {
      applyQuestion(suggestQuestion(lang)); // graceful fallback to the static bank
    } finally {
      setAiBusy(false);
    }
  };

  const save = () => {
    if (!valid) return;
    onSave({ text: text.trim(), options: opts.map((o) => o.trim()).filter(Boolean), multi });
    onClose();
  };

  return (
    <Modal open={open} onClose={onClose} labelledBy="qm-title">
      <div className="modal-head">
        <div>
          <h2 id="qm-title">{t.newQuestion}</h2>
          <div className="mh-sub">{t.newQuestionSub}</div>
        </div>
        <button className="modal-close" onClick={onClose} aria-label="Close"><Icon.x /></button>
      </div>

      <div className="suggest-bar">
        <button className="suggest-btn" onClick={suggestRandom} disabled={aiBusy}>
          <span className={`suggest-ic ${diceSpin ? "spin" : ""}`}><Icon.dice width="17" height="17" /></span>
          {t.suggest}
        </button>
        <button className="suggest-btn ai" onClick={suggestAI} disabled={aiBusy}>
          {aiBusy ? <><span className="ai-spin"></span>{t.aiThinking}</> : <><Icon.spark width="16" height="16" />{t.aiSuggest}</>}
        </button>
      </div>

      <div className="modal-body">
        <label className="field-lbl">{t.questionLabel}</label>
        <input
          className="input big"
          placeholder={t.questionPh}
          value={text}
          autoFocus
          onChange={(e) => setText(e.target.value)}
          onKeyDown={(e) => { if (e.key === "Enter") document.getElementById("opt-0")?.focus(); }}
        />

        <label className="field-lbl" style={{ marginTop: 20 }}>{t.answersLabel}</label>
        {opts.map((o, i) => (
          <div className="opt-row" key={i}>
            <div className="opt-badge" style={{ background: `linear-gradient(140deg, ${OPT_COLORS[i]}, color-mix(in oklch, ${OPT_COLORS[i]}, #000 16%))` }}>{ALPHA[i]}</div>
            <input
              id={`opt-${i}`}
              className="input"
              placeholder={t.optionPh(ALPHA[i])}
              value={o}
              onChange={(e) => setOpt(i, e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  if (i === opts.length - 1 && opts.length < 6) addOpt();
                  else document.getElementById(`opt-${i + 1}`)?.focus();
                }
              }}
            />
            <button className="opt-remove" onClick={() => rmOpt(i)} disabled={opts.length <= 2} aria-label="Remove option"><Icon.x /></button>
          </div>
        ))}
        {opts.length < 6 && (
          <button className="lnk" style={{ marginTop: 2 }} onClick={addOpt}><Icon.plus width="15" height="15" />{t.addOption}</button>
        )}

        <div className="toggle-row">
          <div className="tr-main">
            <div className="tr-title">{t.allowMulti}</div>
            <div className="tr-sub">{t.allowMultiSub}</div>
          </div>
          <button className={`switch ${multi ? "on" : ""}`} onClick={() => setMulti((m) => !m)} role="switch" aria-checked={multi}><i></i></button>
        </div>
      </div>

      <div className="modal-foot">
        <button className="btn btn-ghost grow" onClick={onClose}>{t.cancel}</button>
        <button className="btn btn-primary grow" onClick={save} disabled={!valid}>{t.saveQuestion}</button>
      </div>
    </Modal>
  );
}
