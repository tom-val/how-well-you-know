import { createBrowserRouter, Navigate } from "react-router-dom";
import { lazy, Suspense } from "react";
import type { ReactNode } from "react";
import { AppLayout } from "./components/AppLayout";
import { RequireAuth } from "./components/RequireAuth";
import { Spinner } from "./components/ui";
import { useT } from "./i18n/lang";

const Login = lazy(() => import("./features/auth/Login"));
const Register = lazy(() => import("./features/auth/Register"));
const LobbyPage = lazy(() => import("./features/lobby/LobbyPage"));
const GamesListPage = lazy(() => import("./features/lobby/GamesListPage"));
const GamePage = lazy(() => import("./features/game/GamePage"));
const PlayPage = lazy(() => import("./features/game/PlayPage"));
const ResultsPage = lazy(() => import("./features/game/ResultsPage"));
const QuestionReviewPage = lazy(() => import("./features/game/QuestionReviewPage"));
const JoinByLinkPage = lazy(() => import("./features/game/JoinByLinkPage"));

function Lazy({ children }: { children: ReactNode }) {
  const t = useT();
  return <Suspense fallback={<Spinner t={t} />}>{children}</Suspense>;
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
      { path: "games", element: <Lazy><GamesListPage /></Lazy> },
      { path: "games/:id", element: <Lazy><GamePage /></Lazy> },
      { path: "games/:id/play", element: <Lazy><PlayPage /></Lazy> },
      { path: "games/:id/results", element: <Lazy><ResultsPage /></Lazy> },
      { path: "games/:id/q/:qi", element: <Lazy><QuestionReviewPage /></Lazy> },
      { path: "join/:id", element: <Lazy><JoinByLinkPage /></Lazy> },
    ],
  },
  { path: "*", element: <Navigate to="/" replace /> },
]);
