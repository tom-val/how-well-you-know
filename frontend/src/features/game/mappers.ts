import type { GameStatus, Player } from "../../api/gamesApi";
import type { GameStatusUi, DisplayPlayer } from "../../components/ui";
import { playerColor } from "../../components/ui";

export function uiStatus(status: GameStatus): GameStatusUi {
  if (status === "Started") return "live";
  if (status === "Ended") return "done";
  return "draft";
}

export function toDisplayPlayer(p: Player): DisplayPlayer {
  return { id: p.id, name: p.userName, color: playerColor(p.id) };
}
