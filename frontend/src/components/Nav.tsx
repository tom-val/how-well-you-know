import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useLang } from "../i18n/lang";
import { useAuth } from "../hooks/useAuth";
import { Avatar, Icon, playerColor } from "./ui";

export function Nav() {
  const { lang, setLang, t } = useLang();
  const { user, signOut } = useAuth();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const h = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", h);
    return () => document.removeEventListener("mousedown", h);
  }, []);

  const name = user?.displayName || user?.email?.split("@")[0] || "?";
  const color = playerColor(user?.sub ?? "me");

  return (
    <nav className="nav">
      <div className="nav-inner">
        <button className="brand" onClick={() => navigate("/")}>
          <span className="brand-mark"><span>?</span></span>
          <span className="brand-name">{t.brand}</span>
        </button>
        <div className="nav-right">
          <div className="seg" role="tablist" aria-label="Language">
            <div className="seg-thumb" style={{ transform: lang === "en" ? "translateX(100%)" : "translateX(0)" }}></div>
            <button className={lang === "lt" ? "on" : ""} onClick={() => setLang("lt")}>LT</button>
            <button className={lang === "en" ? "on" : ""} onClick={() => setLang("en")}>EN</button>
          </div>
          <div className={`acct ${open ? "open" : ""}`} ref={ref}>
            <button className="acct-btn" onClick={() => setOpen((o) => !o)}>
              <Avatar name={name} color={color} size={28} />
              <span className="acct-email">{name}</span>
              <span className="acct-caret"><Icon.caret /></span>
            </button>
            {open && (
              <div className="menu">
                <div className="menu-head">
                  <div className="lbl">{t.signedInAs}</div>
                  <div className="val">{user?.email}</div>
                </div>
                <button className="menu-item danger" onClick={signOut}>
                  <Icon.signout />
                  {t.signOut}
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}
