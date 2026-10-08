import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // Where the .NET API runs while you develop. Change it in client/.env.local if yours differs:
  //   VITE_PROXY_TARGET=https://localhost:7178
  const env = loadEnv(mode, '.', '')
  const apiTarget = env.VITE_PROXY_TARGET || 'http://localhost:5198'

  return {
    plugins: [react()],
    server: {
      // Stripe sends people back to this address, so it must never change silently.
      port: 5173,
      strictPort: true,
      proxy: {
        // The browser only talks to localhost:5173. Vite forwards /api to the backend,
        // so the login cookies belong to the same site and no CORS is involved.
        '/api': { target: apiTarget, changeOrigin: false, secure: false },
      },
    },
  }
})
