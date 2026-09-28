const config = {
  APP_NAME: import.meta.env.VITE_APP_NAME || "ERP System",
  API_URL:
    import.meta.env.VITE_API_URL ||
    import.meta.env.VITE_API_BASE_URL ||
    "http://localhost:5252/api",
};

export default config;
