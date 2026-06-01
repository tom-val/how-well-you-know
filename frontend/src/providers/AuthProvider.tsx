import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";
import type { ReactNode } from "react";
import {
  CognitoUserPool,
  CognitoUser,
  AuthenticationDetails,
  CognitoUserAttribute,
  CognitoUserSession,
} from "amazon-cognito-identity-js";
import { setTokenProvider } from "../api/client";

const POOL_ID = import.meta.env.VITE_COGNITO_USER_POOL_ID ?? "";
const CLIENT_ID = import.meta.env.VITE_COGNITO_CLIENT_ID ?? "";
const isCognitoConfigured = POOL_ID !== "" && CLIENT_ID !== "";

const userPool = isCognitoConfigured
  ? new CognitoUserPool({ UserPoolId: POOL_ID, ClientId: CLIENT_ID })
  : null;

export interface AuthUser {
  sub: string;
  email: string;
  displayName: string;
}

export interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  signIn: (email: string, password: string) => Promise<void>;
  signUp: (email: string, password: string, displayName: string) => Promise<void>;
  confirmSignUp: (email: string, code: string) => Promise<void>;
  signOut: () => void;
  getAccessToken: () => Promise<string | null>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const extractUser = useCallback((session: CognitoUserSession): AuthUser => {
    const payload = session.getIdToken().decodePayload();
    return {
      sub: payload.sub,
      email: payload.email,
      displayName: payload.name ?? payload.email,
    };
  }, []);

  useEffect(() => {
    if (!userPool) {
      setIsLoading(false);
      return;
    }
    const cognitoUser = userPool.getCurrentUser();
    if (!cognitoUser) {
      setIsLoading(false);
      return;
    }
    cognitoUser.getSession((err: Error | null, session: CognitoUserSession | null) => {
      if (!err && session?.isValid()) {
        setUser(extractUser(session));
      }
      setIsLoading(false);
    });
  }, [extractUser]);

  const signIn = useCallback(
    async (email: string, password: string) => {
      if (!userPool) throw new Error("Cognito is not configured.");
      const cognitoUser = new CognitoUser({ Username: email, Pool: userPool });
      const authDetails = new AuthenticationDetails({ Username: email, Password: password });
      return new Promise<void>((resolve, reject) => {
        cognitoUser.authenticateUser(authDetails, {
          onSuccess: (session) => {
            setUser(extractUser(session));
            resolve();
          },
          onFailure: reject,
        });
      });
    },
    [extractUser],
  );

  const signUp = useCallback(
    async (email: string, password: string, displayName: string) => {
      if (!userPool) throw new Error("Cognito is not configured.");
      const attributes = [
        new CognitoUserAttribute({ Name: "email", Value: email }),
        new CognitoUserAttribute({ Name: "name", Value: displayName }),
      ];
      return new Promise<void>((resolve, reject) => {
        userPool.signUp(email, password, attributes, [], (err) => {
          if (err) reject(err);
          else resolve();
        });
      });
    },
    [],
  );

  const confirmSignUp = useCallback(async (email: string, code: string) => {
    if (!userPool) throw new Error("Cognito is not configured.");
    const cognitoUser = new CognitoUser({ Username: email, Pool: userPool });
    return new Promise<void>((resolve, reject) => {
      cognitoUser.confirmRegistration(code, true, (err) => {
        if (err) reject(err);
        else resolve();
      });
    });
  }, []);

  const signOut = useCallback(() => {
    userPool?.getCurrentUser()?.signOut();
    setUser(null);
  }, []);

  const getAccessToken = useCallback(async (): Promise<string | null> => {
    const cognitoUser = userPool?.getCurrentUser();
    if (!cognitoUser) return null;
    return new Promise((resolve) => {
      cognitoUser.getSession((err: Error | null, session: CognitoUserSession | null) => {
        resolve(!err && session?.isValid() ? session.getAccessToken().getJwtToken() : null);
      });
    });
  }, []);

  useEffect(() => {
    setTokenProvider(getAccessToken);
  }, [getAccessToken]);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: !!user,
      isLoading,
      signIn,
      signUp,
      confirmSignUp,
      signOut,
      getAccessToken,
    }),
    [user, isLoading, signIn, signUp, confirmSignUp, signOut, getAccessToken],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
