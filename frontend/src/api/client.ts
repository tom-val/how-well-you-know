import axios from "axios";

// In dev, use relative URLs so requests go through the Vite proxy (same-origin, no CORS).
// In a production build, call the API directly via its absolute URL.
const apiClient = axios.create({
  baseURL: import.meta.env.DEV ? "" : (import.meta.env.VITE_API_URL ?? ""),
});

let tokenProvider: (() => Promise<string | null>) | null = null;

/**
 * Registers how the client obtains the current access token. Called once by the
 * AuthProvider; the single interceptor below reads through this each request.
 */
export function setTokenProvider(getToken: () => Promise<string | null>): void {
  tokenProvider = getToken;
}

apiClient.interceptors.request.use(async (config) => {
  const token = tokenProvider ? await tokenProvider() : null;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
