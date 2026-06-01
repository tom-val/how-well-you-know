import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const apiTarget = env.VITE_API_URL ?? "";

  return {
    server: {
      port: 5173,
      // Proxy API calls through Vite in dev so the browser stays same-origin
      // (the deployed API's CORS only allows the CloudFront origin, not localhost).
      proxy: apiTarget
        ? {
            "/v1": { target: apiTarget, changeOrigin: true },
          }
        : undefined,
    },
    // amazon-cognito-identity-js references `global`, which the browser doesn't define.
    define: {
      global: "globalThis",
    },
    plugins: [react()],
  };
});
