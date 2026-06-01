import apiClient from "./client";

export type GameStatus = "Created" | "Started" | "Ended";
export type QuestionPhase = "Answering" | "Review";

export interface Variant {
  id: string;
  notation: string;
  text: string;
}

export interface Question {
  id: string;
  text: string;
  multipleAnswers: boolean;
  createdByUser: string;
  answered: boolean;
  variants: Variant[];
}

export interface Game {
  id: string;
  name: string;
  status: GameStatus;
  currentQuestionPhase: QuestionPhase;
  currentQuestionId: string;
  createdByUser: string;
  players: Player[];
  questions: Question[];
  viewer: ViewerState | null;
  awaitingPlayerIds: string[];
  readyUserIds: string[];
}

export interface Player {
  id: string;
  userName: string;
  profileUrl: string | null;
}

export interface ViewerState {
  hasAnswered: boolean;
  guessedUserIds: string[];
}

export interface GameSummary {
  gameId: string;
  name: string;
  status: GameStatus;
  createdByUser: string;
  createdAt: string;
  questionCount: number;
  playerCount: number;
}

export interface ScoreResult {
  userId: string;
  totalScore: number;
  rank: number;
}

export interface VariantRef {
  notation: string;
  text: string;
}

export interface AnswerReveal {
  userId: string;
  picked: VariantRef[];
}

export interface GuessResult {
  choiceUserId: string;
  score: number;
  guessed: VariantRef[];
}

export interface PlayerQuestionResult {
  userId: string;
  totalScore: number;
  guesses: GuessResult[];
}

export interface QuestionResult {
  questionId: string;
  text: string;
  answers: AnswerReveal[];
  players: PlayerQuestionResult[];
}

export interface GameResults {
  overall: ScoreResult[];
  questions: QuestionResult[];
}

export async function listMyGames(): Promise<GameSummary[]> {
  const { data } = await apiClient.get<GameSummary[]>("/v1/games/mine");
  return data;
}

export async function getGame(id: string): Promise<Game> {
  const { data } = await apiClient.get<Game>(`/v1/games/${id}`);
  return data;
}

export async function getResults(id: string): Promise<GameResults> {
  const { data } = await apiClient.get<GameResults>(`/v1/games/${id}/results`);
  return data;
}

export async function createGame(name: string): Promise<Game> {
  const { data } = await apiClient.post<Game>("/v1/games", { name });
  return data;
}

export async function joinGame(id: string): Promise<Game> {
  const { data } = await apiClient.post<Game>(`/v1/games/${id}/join`);
  return data;
}

// --- Used by the gameplay screens (next iteration) ---

export async function addQuestion(
  gameId: string,
  text: string,
  multipleAnswers: boolean,
  variants: Record<string, string>,
): Promise<Question> {
  const { data } = await apiClient.post<Question>(`/v1/games/${gameId}/questions`, {
    text,
    multipleAnswers,
    variants,
  });
  return data;
}

export async function deleteQuestion(gameId: string, questionId: string): Promise<Game> {
  const { data } = await apiClient.delete<Game>(`/v1/games/${gameId}/questions/${questionId}`);
  return data;
}

/** Marks the current player ready/not-ready in the lobby. The game auto-starts once all are ready. */
export async function setReady(id: string, ready: boolean): Promise<Game> {
  const { data } = await apiClient.post<Game>(`/v1/games/${id}/ready`, { ready });
  return data;
}

export async function recordChoice(id: string, variantNotations: string[]): Promise<Game> {
  const { data } = await apiClient.post<Game>(`/v1/games/${id}/choices`, { variantNotations });
  return data;
}

export async function recordGuess(
  id: string,
  choiceUserId: string,
  variantNotations: string[],
): Promise<Game> {
  const { data } = await apiClient.post<Game>(`/v1/games/${id}/guesses`, {
    choiceUserId,
    variantNotations,
  });
  return data;
}

export async function advanceQuestion(id: string): Promise<Game> {
  const { data } = await apiClient.post<Game>(`/v1/games/${id}/advance`);
  return data;
}
