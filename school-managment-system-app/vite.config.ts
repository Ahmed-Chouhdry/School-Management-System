import { defineConfig } from 'vitest/config' // We import from 'vitest/config' to get the proper types
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './vitest.setup.ts', // Points to your test setup file
  },
})