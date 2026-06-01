import apiClient from "./client";

export interface User {
  id: string;
  userName: string;
  profileUrl: string | null;
}

/** Registers the current authenticated user with a display name (idempotent). */
export async function registerMe(userName: string): Promise<User> {
  const { data } = await apiClient.post<User>("/v1/users", { userName });
  return data;
}

export async function getMe(): Promise<User | null> {
  try {
    const { data } = await apiClient.get<User>("/v1/users/me");
    return data;
  } catch (err) {
    if (isStatus(err, 404)) return null;
    throw err;
  }
}

export async function getUser(id: string): Promise<User> {
  const { data } = await apiClient.get<User>(`/v1/users/${id}`);
  return data;
}

export function isStatus(err: unknown, status: number): boolean {
  return (
    typeof err === "object" &&
    err !== null &&
    "response" in err &&
    (err as { response?: { status?: number } }).response?.status === status
  );
}
