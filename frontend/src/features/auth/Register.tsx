import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useSnackbar } from "notistack";
import { useAuth } from "../../hooks/useAuth";
import { LanguageSwitcher } from "../../components/LanguageSwitcher";

export default function Register() {
  const { t } = useTranslation();
  const { signUp, confirmSignUp } = useAuth();
  const { enqueueSnackbar } = useSnackbar();
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [code, setCode] = useState("");
  const [awaitingCode, setAwaitingCode] = useState(false);
  const [busy, setBusy] = useState(false);

  async function onSignUp(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    try {
      await signUp(email, password, displayName);
      setAwaitingCode(true);
    } catch (err) {
      enqueueSnackbar(err instanceof Error ? err.message : t("common.error"), {
        variant: "error",
      });
    } finally {
      setBusy(false);
    }
  }

  async function onConfirm(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    try {
      await confirmSignUp(email, code);
      enqueueSnackbar(t("auth.confirmedNowSignIn"), { variant: "success" });
      navigate("/login", { replace: true });
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

        {!awaitingCode ? (
          <>
            <h1>{t("auth.signUp")}</h1>
            <form onSubmit={onSignUp} className="form">
              <label>
                {t("auth.displayNameLabel")}
                <input
                  type="text"
                  value={displayName}
                  onChange={(e) => setDisplayName(e.target.value)}
                  autoComplete="nickname"
                  required
                />
              </label>
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
                  autoComplete="new-password"
                  required
                />
              </label>
              <button type="submit" className="primary" disabled={busy}>
                {busy ? t("auth.signingUp") : t("auth.signUp")}
              </button>
            </form>
            <p className="muted">
              {t("auth.haveAccount")} <Link to="/login">{t("auth.signIn")}</Link>
            </p>
          </>
        ) : (
          <>
            <h1>{t("auth.confirmTitle")}</h1>
            <p className="muted">{t("auth.confirmHint")}</p>
            <form onSubmit={onConfirm} className="form">
              <label>
                {t("auth.codeLabel")}
                <input
                  type="text"
                  inputMode="numeric"
                  value={code}
                  onChange={(e) => setCode(e.target.value)}
                  required
                />
              </label>
              <button type="submit" className="primary" disabled={busy}>
                {busy ? t("auth.confirming") : t("auth.confirm")}
              </button>
            </form>
          </>
        )}
      </div>
    </div>
  );
}
