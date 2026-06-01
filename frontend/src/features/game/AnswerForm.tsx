import { useState } from "react";
import type { FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { Question } from "../../api/gamesApi";

interface Props {
  question: Question;
  submitLabel: string;
  submitting: boolean;
  onSubmit: (variantNotations: string[]) => void;
}

/** Single- or multiple-choice picker over a question's variants. */
export function AnswerForm({ question, submitLabel, submitting, onSubmit }: Props) {
  const { t } = useTranslation();
  const [selected, setSelected] = useState<string[]>([]);

  function toggle(notation: string) {
    if (question.multipleAnswers) {
      setSelected((cur) =>
        cur.includes(notation) ? cur.filter((n) => n !== notation) : [...cur, notation],
      );
    } else {
      setSelected([notation]);
    }
  }

  function submit(e: FormEvent) {
    e.preventDefault();
    if (selected.length > 0) onSubmit(selected);
  }

  return (
    <form onSubmit={submit} className="answer-form">
      <ul className="variant-list">
        {question.variants.map((v) => {
          const checked = selected.includes(v.notation);
          return (
            <li key={v.id}>
              <label className={`variant-option ${checked ? "selected" : ""}`}>
                <input
                  type={question.multipleAnswers ? "checkbox" : "radio"}
                  name={`q-${question.id}`}
                  checked={checked}
                  onChange={() => toggle(v.notation)}
                />
                <span className="variant-notation">{v.notation}</span>
                <span>{v.text}</span>
              </label>
            </li>
          );
        })}
      </ul>
      <button type="submit" className="primary" disabled={submitting || selected.length === 0}>
        {submitting ? t("common.loading") : submitLabel}
      </button>
    </form>
  );
}
