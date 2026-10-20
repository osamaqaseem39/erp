/// <reference types="react-scripts" />

interface ImportMetaEnv {
  readonly REACT_APP_SOLSCAN_API_KEY: string;
  readonly REACT_APP_API_BASE_URL: string;
  // Add other env variables here if needed
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
} 