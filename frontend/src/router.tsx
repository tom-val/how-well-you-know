import { createBrowserRouter, Navigate } from "react-router-dom";
import { lazy, Suspense } from "react";
import type { ReactNode } from "react";
import { AppLayout } from "./components/AppLayout";
import { RequireAuth } from "./components/RequireAuth";
import { Spinner } from "./components/Spinner";

const Login = lazy(() => import("./features/auth/Login"));
const Register = lazy(() => import("./features/auth/Register"));
const LobbyPage = lazy(() => import("./features/lobby/LobbyPage"));
const GamePage = lazy(() => import("./features/game/GamePage"));
const JoinByLinkPage = lazy(() => import("./features/game/JoinByLinkPage"));

function Lazy({ children }: { children: ReactNode }) {
  return <Suspense fallback={<Spinner />}>{children}</Suspense>;
}

export const router = createBrowserRouter([
  { path: "/login", element: <Lazy><Login /></Lazy> },
  { path: "/register", element: <Lazy><Register /></Lazy> },
  {
    path: "/",
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    children: [
      { index: true, element: <Lazy><LobbyPage /></Lazy> },
      { path: "join/:id", element: <Lazy><JoinByLinkPage /></Lazy> },
      { path: "games/:id", element: <Lazy><GamePage /></Lazy> },
    ],
  },
  { path: "*", element: <Navigate to="/" replace /> },
]);
