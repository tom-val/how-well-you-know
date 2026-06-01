import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useSnackbar } from "notistack";
import { useAuth } from "../../hooks/useAuth";
import { LanguageSwitcher } from "../../components/LanguageSwitcher";

interface LocationState {
  from?: { pathname: string };
}

export default function Login() {
  const { t } = useTranslation();
  const { signIn } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);

  const redirectTo = (location.state as LocationState)?.from?.pathname ?? "/";

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    try {
      await signIn(email, password);
      navigate(redirectTo, { replace: true });
    } catch (err) {
      enqueueSnackbar(err instanceof Error ? err.message : t("common.error"), {
        variant: "error",
      });
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-top">
          <LanguageSwitcher />
        </div>
        <h1>{t("app.title")}</h1>
        <p className="muted">{t("app.tagline")}</p>
        <form onSubmit={onSubmit} className="form">
          <label>
            {t("auth.emailLabel")}
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              autoComplete="email"
              required
            />
          </label>
          <label>
            {t("auth.passwordLabel")}
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
              required
            />
          </label>
          <button type="submit" className="primary" disabled={busy}>
            {busy ? t("auth.signingIn") : t("auth.signIn")}
          </button>
        </form>
        <p className="muted">
          {t("auth.noAccount")} <Link to="/register">{t("auth.signUp")}</Link>
        </p>
      </div>
    </div>
  );
}
