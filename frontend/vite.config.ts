import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  define: { 'process.env': {} },
  test: {
    environment: 'node',
    include: ['src/**/*.test.ts'],
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@/modules': path.resolve(__dirname, './src/modules'),
      '@/components': path.resolve(__dirname, './src/components'),
      '@/services': path.resolve(__dirname, './src/services'),
      '@/types': path.resolve(__dirname, './src/types'),
      '@/hooks': path.resolve(__dirname, './src/hooks'),
      '@/utils': path.resolve(__dirname, './src/utils'),
      '@/helpers': path.resolve(__dirname, './src/helpers'),
      '@/redux': path.resolve(__dirname, './src/redux'),
      '@/routes': path.resolve(__dirname, './src/routes'),
      '@/layouts': path.resolve(__dirname, './src/layouts'),
      '@/pages': path.resolve(__dirname, './src/pages'),
    },
  },
})
