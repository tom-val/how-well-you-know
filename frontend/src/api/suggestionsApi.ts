import apiClient from "./client";

export interface AiSuggestion {
  text: string;
  options: string[];
}

/** Asks the API for an AI-generated question. Throws on failure (caller falls back to the static bank). */
export async function suggestAiQuestion(language: string, topic?: string): Promise<AiSuggestion> {
  const { data } = await apiClient.post<AiSuggestion>("/v1/questions/suggest", { language, topic });
  return data;
}
