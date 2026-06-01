import { useEffect } from "react";
import type { ReactNode, SVGProps } from "react";
import type { T } from "../i18n/strings";

/* ---- player accent colours ---- */
const AV_COLORS: Record<string, string> = {
  violet: "var(--c-violet)",
  coral: "var(--c-coral)",
  mint: "var(--c-mint)",
  amber: "var(--c-amber)",
  sky: "var(--c-sky)",
  pink: "var(--c-pink)",
};
const PALETTE = ["violet", "coral", "mint", "amber", "sky", "pink"] as const;

export function avColor(c?: string): string {
  return (c && AV_COLORS[c]) || c || "var(--c-violet)";
}

/** Stable accent colour for a real player id (backend has no colour field). */
export function playerColor(id: string): string {
  let h = 0;
  for (let i = 0; i < id.length; i++) h = (h * 31 + id.charCodeAt(i)) >>> 0;
  return PALETTE[h % PALETTE.length];
}

export function initials(name?: string): string {
  if (!name) return "?";
  const p = name.trim().split(/\s+/);
  return (p[0][0] + (p[1] ? p[1][0] : "")).toUpperCase();
}

/* ---- Icons ---- */
type IP = SVGProps<SVGSVGElement>;
export const Icon = {
  caret: (p: IP) => <svg width="14" height="14" viewBox="0 0 24 24" fill="none" {...p}><path d="M6 9l6 6 6-6" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  arrowL: (p: IP) => <svg width="18" height="18" viewBox="0 0 24 24" fill="none" {...p}><path d="M15 5l-7 7 7 7" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  arrowR: (p: IP) => <svg width="18" height="18" viewBox="0 0 24 24" fill="none" {...p}><path d="M9 5l7 7-7 7" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  plus: (p: IP) => <svg width="17" height="17" viewBox="0 0 24 24" fill="none" {...p}><path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" /></svg>,
  copy: (p: IP) => <svg width="16" height="16" viewBox="0 0 24 24" fill="none" {...p}><rect x="9" y="9" width="11" height="11" rx="3" stroke="currentColor" strokeWidth="2" /><path d="M5 15V6a2 2 0 0 1 2-2h8" stroke="currentColor" strokeWidth="2" strokeLinecap="round" /></svg>,
  check: (p: IP) => <svg width="16" height="16" viewBox="0 0 24 24" fill="none" {...p}><path d="M4 12.5l5 5 11-11" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  x: (p: IP) => <svg width="16" height="16" viewBox="0 0 24 24" fill="none" {...p}><path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" /></svg>,
  trash: (p: IP) => <svg width="16" height="16" viewBox="0 0 24 24" fill="none" {...p}><path d="M4 7h16M9 7V5a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2m2 0v12a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2V7" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  signout: (p: IP) => <svg width="16" height="16" viewBox="0 0 24 24" fill="none" {...p}><path d="M15 12H4m0 0l4-4m-4 4l4 4M14 4h4a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  trophy: (p: IP) => <svg width="20" height="20" viewBox="0 0 24 24" fill="none" {...p}><path d="M7 4h10v4a5 5 0 0 1-10 0V4zM7 6H4v1a3 3 0 0 0 3 3M17 6h3v1a3 3 0 0 1-3 3M9 18h6M10 18v-2.5M14 18v-2.5M8 21h8" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  link: (p: IP) => <svg width="16" height="16" viewBox="0 0 24 24" fill="none" {...p}><path d="M10 14a4 4 0 0 0 6 .5l2-2a4 4 0 0 0-5.7-5.7l-1 1M14 10a4 4 0 0 0-6-.5l-2 2A4 4 0 0 0 11.7 17l1-1" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  spark: (p: IP) => <svg width="18" height="18" viewBox="0 0 24 24" fill="none" {...p}><path d="M12 3l1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8L12 3z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" /></svg>,
  dice: (p: IP) => <svg width="18" height="18" viewBox="0 0 24 24" fill="none" {...p}><rect x="4" y="4" width="16" height="16" rx="4" stroke="currentColor" strokeWidth="2" /><circle cx="9" cy="9" r="1.4" fill="currentColor" /><circle cx="15" cy="15" r="1.4" fill="currentColor" /><circle cx="15" cy="9" r="1.4" fill="currentColor" /><circle cx="9" cy="15" r="1.4" fill="currentColor" /></svg>,
  search: (p: IP) => <svg width="18" height="18" viewBox="0 0 24 24" fill="none" {...p}><circle cx="11" cy="11" r="6.5" stroke="currentColor" strokeWidth="2" /><path d="M16 16l4 4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" /></svg>,
};

/* ---- Avatar ---- */
export function Avatar({ name, color, size = 36, ring }: { name?: string; color?: string; size?: number; ring?: boolean }) {
  return (
    <div
      className="av"
      style={{
        width: size,
        height: size,
        fontSize: size * 0.4,
        background: `linear-gradient(140deg, ${avColor(color)}, color-mix(in oklch, ${avColor(color)}, #000 14%))`,
        boxShadow: ring ? `0 0 0 3px var(--surface), 0 0 0 5px ${avColor(color)}` : undefined,
      }}
    >
      {initials(name)}
    </div>
  );
}

export interface DisplayPlayer {
  id: string;
  name: string;
  color?: string;
}

export function AvatarStack({ players, size = 30, max = 5 }: { players: DisplayPlayer[]; size?: number; max?: number }) {
  const show = players.slice(0, max);
  const extra = players.length - show.length;
  return (
    <div className="row" style={{ paddingLeft: 6 }}>
      {show.map((p, i) => (
        <div key={p.id} style={{ marginLeft: -8, zIndex: show.length - i, boxShadow: "0 0 0 3px var(--surface)", borderRadius: "50%" }}>
          <Avatar name={p.name} color={p.color} size={size} />
        </div>
      ))}
      {extra > 0 && (
        <div className="av" style={{ marginLeft: -8, width: size, height: size, fontSize: size * 0.36, background: "var(--surface-3)", color: "var(--ink-soft)", boxShadow: "0 0 0 3px var(--surface)" }}>
          +{extra}
        </div>
      )}
    </div>
  );
}

/* ---- Status badge ---- */
export type GameStatusUi = "live" | "draft" | "done";
export function StatusBadge({ status, t }: { status: GameStatusUi; t: T }) {
  const map: Record<GameStatusUi, [string, string]> = {
    live: ["live", t.st_live],
    draft: ["draft", t.st_draft],
    done: ["done", t.st_done],
  };
  const [cls, label] = map[status] || map.draft;
  return (
    <span className={`badge ${cls}`}>
      <span className="dot"></span>
      {label}
    </span>
  );
}

/* ---- Modal shell ---- */
export function Modal({ open, onClose, children, labelledBy }: { open: boolean; onClose: () => void; children: ReactNode; labelledBy?: string }) {
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => { if (e.key === "Escape") onClose(); };
    document.addEventListener("keydown", onKey);
    document.body.style.overflow = "hidden";
    return () => { document.removeEventListener("keydown", onKey); document.body.style.overflow = ""; };
  }, [open, onClose]);
  if (!open) return null;
  return (
    <div className="modal-scrim" onMouseDown={onClose}>
      <div className="modal" role="dialog" aria-modal="true" aria-labelledby={labelledBy} onMouseDown={(e) => e.stopPropagation()}>
        {children}
      </div>
    </div>
  );
}

/* ---- Back-link header ---- */
export function ScreenHead({ onBack, title, right, t }: { onBack: () => void; title: ReactNode; right?: ReactNode; t: T }) {
  return (
    <div className="screen-head enter">
      <div className="between" style={{ marginBottom: 6 }}>
        <button className="lnk" onClick={onBack}>
          <Icon.arrowL />
          {t.back}
        </button>
        {right}
      </div>
      <h1 className="screen-title">{title}</h1>
    </div>
  );
}

/* ---- Spinner (used by route fallbacks / loading) ---- */
export function Spinner({ t }: { t: T }) {
  return (
    <div className="page" style={{ display: "grid", placeItems: "center", minHeight: "40vh" }}>
      <div className="wait-spinner"><i></i></div>
      <div className="muted" style={{ marginTop: 14 }}>{t.loading}</div>
    </div>
  );
}
