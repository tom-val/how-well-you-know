import { useEffect } from "react";
import { useAuth } from "./useAuth";
import { registerMe } from "../api/usersApi";

/**
 * Ensures the authenticated user has a domain profile. The API needs a registered
 * user before creating/joining games; registration is idempotent, so we just call it
 * once after sign-in using the display name from the token.
 */
export function useEnsureRegistered() {
  const { isAuthenticated, user } = useAuth();

  useEffect(() => {
    if (isAuthenticated && user) {
      registerMe(user.displayName).catch(() => {
        // Best-effort; surfaced later if a game action then fails.
      });
    }
  }, [isAuthenticated, user]);
}
