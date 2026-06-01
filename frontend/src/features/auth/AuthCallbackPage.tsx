import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useLang } from "../../i18n/lang";
import { Spinner } from "../../components/ui";
import { completeGoogleLogin } from "../../api/cognitoOauth";
import { AuthCard } from "./AuthCard";

export default function AuthCallbackPage() {
  const { t } = useLang();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("code");
    const err = params.get("error_description") || params.get("error");
    if (err) { setError(err); return; }
    if (!code) { setError(t.error); return; }

    completeGoogleLogin(code)
      .then((returnTo) => {
        // Full reload so the AuthProvider re-reads the freshly stored session.
        window.location.replace(returnTo);
      })
      .catch((e) => setError(e instanceof Error ? e.message : t.error));
  }, [t]);

  if (error) {
    return (
      <AuthCard title={t.error} subtitle={error}>
        <Link to="/login" className="btn btn-primary btn-block">{t.signIn}</Link>
      </AuthCard>
    );
  }

  return <Spinner t={t} />;
}
