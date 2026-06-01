import { Link, Outlet } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../hooks/useAuth";
import { useEnsureRegistered } from "../hooks/useEnsureRegistered";
import { LanguageSwitcher } from "./LanguageSwitcher";

export function AppLayout() {
  const { t } = useTranslation();
  const { user, signOut } = useAuth();
  useEnsureRegistered();

  return (
    <div className="app-shell">
      <header className="app-header">
        <Link to="/" className="brand">
          {t("app.title")}
        </Link>
        <nav className="app-nav">
          <LanguageSwitcher />
          {user && <span className="user-name">{user.displayName}</span>}
          <button type="button" className="link-btn" onClick={signOut}>
            {t("nav.signOut")}
          </button>
        </nav>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}
