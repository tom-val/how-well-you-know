import type { ReactNode } from "react";
import { useLang } from "../../i18n/lang";

export function AuthCard({ title, subtitle, children }: { title: string; subtitle?: string; children: ReactNode }) {
  const { lang, setLang } = useLang();
  return (
    <div className="page" style={{ maxWidth: 440, paddingTop: 56 }}>
      <div className="card card-pad enter" style={{ padding: 28 }}>
        <div className="between" style={{ marginBottom: 18 }}>
          <span className="brand-mark"><span>?</span></span>
          <div className="seg" role="tablist" aria-label="Language">
            <div className="seg-thumb" style={{ transform: lang === "en" ? "translateX(100%)" : "translateX(0)" }}></div>
            <button className={lang === "lt" ? "on" : ""} onClick={() => setLang("lt")}>LT</button>
            <button className={lang === "en" ? "on" : ""} onClick={() => setLang("en")}>EN</button>
          </div>
        </div>
        <h1 className="screen-title" style={{ fontSize: 30, marginBottom: 6 }}>{title}</h1>
        {subtitle && <p className="muted" style={{ marginTop: 0, marginBottom: 18, fontSize: 14.5 }}>{subtitle}</p>}
        {children}
      </div>
    </div>
  );
}
