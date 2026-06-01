import { useEffect, useRef } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../hooks/useAuth";

const WS_URL = import.meta.env.VITE_WS_URL ?? "";

/**
 * Opens a WebSocket to the real-time API, subscribes to the given game, and on a
 * "game-changed" signal invalidates the game + results queries so React Query refetches
 * immediately. Reconnects with backoff; a no-op when VITE_WS_URL is unset (e.g. local dev),
 * where the screens' slow safety-poll keeps things eventually consistent.
 */
export function useGameSocket(gameId: string | undefined): void {
  const { getAccessToken } = useAuth();
  const queryClient = useQueryClient();
  const socketRef = useRef<WebSocket | null>(null);

  useEffect(() => {
    if (!WS_URL || !gameId) return;

    let closed = false;
    let attempt = 0;
    let reconnectTimer: number | undefined;

    async function connect() {
      const token = await getAccessToken();
      if (!token || closed) return;

      const ws = new WebSocket(`${WS_URL}?token=${encodeURIComponent(token)}`);
      socketRef.current = ws;

      ws.onopen = () => {
        attempt = 0;
        ws.send(JSON.stringify({ action: "subscribe", gameId }));
      };

      ws.onmessage = (event) => {
        try {
          const msg = JSON.parse(event.data as string);
          if (msg.type === "game-changed" && msg.gameId === gameId) {
            // Prefix match also covers per-question review (["results", id, questionId]).
            queryClient.invalidateQueries({ queryKey: ["game", gameId] });
            queryClient.invalidateQueries({ queryKey: ["results", gameId] });
          }
        } catch {
          /* ignore malformed frames */
        }
      };

      ws.onerror = () => ws.close();

      ws.onclose = () => {
        socketRef.current = null;
        if (closed) return;
        const delay = Math.min(1000 * 2 ** attempt, 15000);
        attempt += 1;
        reconnectTimer = window.setTimeout(connect, delay);
      };
    }

    void connect();

    return () => {
      closed = true;
      if (reconnectTimer) window.clearTimeout(reconnectTimer);
      socketRef.current?.close();
      socketRef.current = null;
    };
  }, [gameId, getAccessToken, queryClient]);
}
