const SetupLoadingScreen = () => (
  <div className="bg-gradient-to-r from-rose-100 to-teal-100 dark:from-gray-700 dark:via-gray-900 dark:to-black min-h-screen">
    <div className="flex items-center justify-center min-h-screen">
      <div className="text-center">
        <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 mb-4">
          <svg
            className="animate-spin h-8 w-8 text-primary"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
        </div>
        <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">Initializing ERP</h3>
        <p className="text-gray-600 dark:text-gray-400">Checking system configuration...</p>
      </div>
    </div>
  </div>
);

export default SetupLoadingScreen;
