import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useSnackbar } from "notistack";
import { useAuth } from "../../hooks/useAuth";
import { useLang } from "../../i18n/lang";
import { AuthCard } from "./AuthCard";

export default function Register() {
  const { t } = useLang();
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
      enqueueSnackbar(err instanceof Error ? err.message : t.error, { variant: "error" });
    } finally {
      setBusy(false);
    }
  }

  async function onConfirm(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    try {
      await confirmSignUp(email, code);
      enqueueSnackbar(t.confirmedNowSignIn, { variant: "success" });
      navigate("/login", { replace: true });
    } catch (err) {
      enqueueSnackbar(err instanceof Error ? err.message : t.error, { variant: "error" });
    } finally {
      setBusy(false);
    }
  }

  if (awaitingCode) {
    return (
      <AuthCard title={t.confirmTitle} subtitle={t.confirmHint}>
        <form className="stack" style={{ gap: 14 }} onSubmit={onConfirm}>
          <div>
            <label className="field-lbl">{t.codeLabel}</label>
            <input className="input" inputMode="numeric" value={code} onChange={(e) => setCode(e.target.value)} required />
          </div>
          <button className="btn btn-primary btn-lg btn-block" type="submit" disabled={busy}>
            {busy ? t.confirming : t.confirm}
          </button>
        </form>
      </AuthCard>
    );
  }

  return (
    <AuthCard title={t.signUp}>
      <form className="stack" style={{ gap: 14 }} onSubmit={onSignUp}>
        <div>
          <label className="field-lbl">{t.displayNameLabel}</label>
          <input className="input" type="text" autoComplete="nickname" value={displayName} onChange={(e) => setDisplayName(e.target.value)} required />
        </div>
        <div>
          <label className="field-lbl">{t.emailLabel}</label>
          <input className="input" type="email" autoComplete="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <div>
          <label className="field-lbl">{t.passwordLabel}</label>
          <input className="input" type="password" autoComplete="new-password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </div>
        <button className="btn btn-primary btn-lg btn-block" type="submit" disabled={busy}>
          {busy ? t.signingUp : t.signUp}
        </button>
      </form>
      <p className="muted" style={{ marginTop: 16, fontSize: 14 }}>
        {t.haveAccount}{" "}
        <Link to="/login" className="lnk" style={{ display: "inline" }}>{t.signIn}</Link>
      </p>
    </AuthCard>
  );
}
