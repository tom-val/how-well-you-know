// Federated (Google) sign-in via the Cognito Hosted UI authorization-code + PKCE flow,
// bridging the resulting tokens into amazon-cognito-identity-js so the rest of the app
// (which uses userPool.getCurrentUser()) works unchanged.

const DOMAIN = import.meta.env.VITE_COGNITO_DOMAIN ?? "";
const CLIENT_ID = import.meta.env.VITE_COGNITO_CLIENT_ID ?? "";

export function isGoogleEnabled(): boolean {
  return DOMAIN !== "" && CLIENT_ID !== "";
}

function redirectUri(): string {
  return `${window.location.origin}/auth/callback`;
}

function base64url(bytes: Uint8Array): string {
  let s = "";
  bytes.forEach((b) => (s += String.fromCharCode(b)));
  return btoa(s).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

async function pkceChallenge(verifier: string): Promise<string> {
  const digest = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(verifier));
  return base64url(new Uint8Array(digest));
}

function decodeJwt(segment: string): Record<string, unknown> {
  const b64 = segment.replace(/-/g, "+").replace(/_/g, "/");
  const pad = b64.length % 4 ? "=".repeat(4 - (b64.length % 4)) : "";
  return JSON.parse(atob(b64 + pad));
}

/** Kick off Google sign-in: redirect to the Hosted UI authorize endpoint. */
export async function loginWithGoogle(returnTo = "/"): Promise<void> {
  const verifierBytes = new Uint8Array(32);
  crypto.getRandomValues(verifierBytes);
  const verifier = base64url(verifierBytes);
  sessionStorage.setItem("pkce_verifier", verifier);
  sessionStorage.setItem("pkce_return", returnTo);

  const params = new URLSearchParams({
    identity_provider: "Google",
    client_id: CLIENT_ID,
    response_type: "code",
    scope: "openid email profile",
    redirect_uri: redirectUri(),
    code_challenge: await pkceChallenge(verifier),
    code_challenge_method: "S256",
  });
  window.location.href = `https://${DOMAIN}/oauth2/authorize?${params.toString()}`;
}

interface TokenResponse {
  id_token: string;
  access_token: string;
  refresh_token: string;
}

/** Exchange the authorization code for tokens and store them as a Cognito SDK session. */
export async function completeGoogleLogin(code: string): Promise<string> {
  const verifier = sessionStorage.getItem("pkce_verifier") ?? "";
  const body = new URLSearchParams({
    grant_type: "authorization_code",
    client_id: CLIENT_ID,
    code,
    redirect_uri: redirectUri(),
    code_verifier: verifier,
  });

  const res = await fetch(`https://${DOMAIN}/oauth2/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
  });
  if (!res.ok) throw new Error("Token exchange failed");
  const tokens = (await res.json()) as TokenResponse;

  const username = String(decodeJwt(tokens.access_token.split(".")[1]).username ?? "");
  const base = `CognitoIdentityServiceProvider.${CLIENT_ID}`;
  localStorage.setItem(`${base}.LastAuthUser`, username);
  localStorage.setItem(`${base}.${username}.idToken`, tokens.id_token);
  localStorage.setItem(`${base}.${username}.accessToken`, tokens.access_token);
  localStorage.setItem(`${base}.${username}.refreshToken`, tokens.refresh_token);
  localStorage.setItem(`${base}.${username}.clockDrift`, "0");

  const returnTo = sessionStorage.getItem("pkce_return") || "/";
  sessionStorage.removeItem("pkce_verifier");
  sessionStorage.removeItem("pkce_return");
  return returnTo;
}
