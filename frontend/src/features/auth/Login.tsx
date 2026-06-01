import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useSnackbar } from "notistack";
import { useAuth } from "../../hooks/useAuth";
import { useLang } from "../../i18n/lang";
import { AuthCard } from "./AuthCard";
import { GoogleButton } from "./GoogleButton";

interface LocationState {
  from?: { pathname: string };
}

export default function Login() {
  const { t } = useLang();
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
      enqueueSnackbar(err instanceof Error ? err.message : t.error, { variant: "error" });
    } finally {
      setBusy(false);
    }
  }

  return (
    <AuthCard title={t.brand} subtitle={t.tagline}>
      <form className="stack" style={{ gap: 14 }} onSubmit={onSubmit}>
        <div>
          <label className="field-lbl">{t.emailLabel}</label>
          <input className="input" type="email" autoComplete="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <div>
          <label className="field-lbl">{t.passwordLabel}</label>
          <input className="input" type="password" autoComplete="current-password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </div>
        <button className="btn btn-primary btn-lg btn-block" type="submit" disabled={busy}>
          {busy ? t.signingIn : t.signIn}
        </button>
      </form>
      <GoogleButton />
      <p className="muted" style={{ marginTop: 16, fontSize: 14 }}>
        {t.noAccount}{" "}
        <Link to="/register" className="lnk" style={{ display: "inline" }}>{t.signUp}</Link>
      </p>
    </AuthCard>
  );
}
