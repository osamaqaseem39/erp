import { jwtDecode } from "jwt-decode";
import axios, { type AxiosRequestConfig } from "axios";

import config from "../../config";

// content type
axios.defaults.headers.post["Content-Type"] = "application/json";
axios.defaults.baseURL = config.API_URL;

// Reject requests when the stored session is missing or expired
axios.interceptors.request.use((config) => {
  const requestUrl = config.url ?? "";
  if (isAuthEndpoint(requestUrl)) {
    return config;
  }

  const stored = getUserFromCookie();
  if (!stored?.token) {
    return config;
  }

  const api = new APICore();
  if (!api.isUserAuthenticated()) {
    handleUnauthenticatedSession("session_expired");
    return Promise.reject(new Error("Session expired"));
  }

  return config;
});

// intercepting to capture errors
axios.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // Any status codes that falls outside the range of 2xx cause this function to trigger
    let message;

    // Check if error has a response (server responded with error status)
    if (error && error.response) {
      const status = error.response.status;
      const requestUrl = error.config?.url ?? "";
      
      if (status === 404) {
        // window.location.href = '/not-found';
        message = "Sorry! the data you are looking for could not be found";
      } else if (status === 403) {
        // Rendered in place by the page (PermissionDenied); a 403 must not take the whole app away.
        message = "You don't have permission to do this.";
      } else if (status === 401) {
        if (!isAuthEndpoint(requestUrl) && !isOnLoginPage()) {
          handleUnauthenticatedSession("unauthorized");
          message = "Session expired. Please sign in again.";
        } else if (isAuthEndpoint(requestUrl) && requestUrl.toLowerCase().includes("/auth/login")) {
          message = "Invalid credentials";
        } else {
          message = "Unauthorized";
        }
      } else {
        // Extract error message from response
        const errorData = error.response.data;
        if (errorData) {
          // Try multiple possible error message fields
          message = errorData.Error || errorData.error || errorData.message || errorData.Message || error.message || "An error occurred";
        } else {
          message = error.message || "An error occurred";
        }
      }
    } else {
      // Network error or no response (server unreachable, CORS, etc.)
      message = error.message || "Network error: Unable to connect to server";
    }
    
    // Preserve the original error object with the message
    const enhancedError = new Error(message);
    (enhancedError as any).originalError = error;
    (enhancedError as any).response = error.response;
    (enhancedError as any).status = error.response?.status;
    
    return Promise.reject(enhancedError);
  }
);

const AUTH_SESSION_KEY = "konrix_user";
const LOGIN_PATH = "/auth/login";

const isAuthEndpoint = (url?: string) => {
  if (!url) return false;
  const path = url.toLowerCase();
  return (
    path.includes("/auth/login") ||
    path.includes("/auth/register") ||
    path.includes("/auth/refresh") ||
    path.includes("/auth/forgot-password") ||
    path.includes("/tenants/login-options") ||
    path.includes("/tenants/domain/") ||
    path.includes("/setup/")
  );
};

const isOnLoginPage = () =>
  typeof window !== "undefined" && window.location.pathname.startsWith(LOGIN_PATH);

/**
 * Sets the default authorization
 * @param {*} token
 */
const setAuthorization = (token: string | null) => {
  if (token) axios.defaults.headers.common["Authorization"] = "Bearer " + token;
  else delete axios.defaults.headers.common["Authorization"];
};

const getUserFromCookie = () => {
  const user = sessionStorage.getItem(AUTH_SESSION_KEY);
  return user ? (typeof user == "object" ? user : JSON.parse(user)) : null;
};

/** Clears stored session and removes the API authorization header. */
const clearAuthSession = () => {
  sessionStorage.removeItem(AUTH_SESSION_KEY);
  setAuthorization(null);
};

/** Clears session and sends the user to the login page (full navigation). */
const forceLogout = (reason?: string) => {
  clearAuthSession();
  if (window.location.pathname.startsWith(LOGIN_PATH)) {
    return;
  }

  const params = new URLSearchParams();
  if (reason) {
    params.set("reason", reason);
  }
  const returnPath = window.location.pathname + window.location.search;
  if (returnPath && returnPath !== LOGIN_PATH) {
    params.set("next", returnPath);
  }
  const query = params.toString();
  window.location.replace(`${LOGIN_PATH}${query ? `?${query}` : ""}`);
};

let logoutInProgress = false;

const handleUnauthenticatedSession = (reason?: string) => {
  if (logoutInProgress) return;
  logoutInProgress = true;
  forceLogout(reason);
};
class APICore {
  /**
   * Fetches data from given url
   */
  get = (url: string, params: any, config?: AxiosRequestConfig) => {
    if (params && typeof params === 'object') {
      // Build query string from params object
      const queryString = Object.keys(params)
        .filter(key => params[key] !== null && params[key] !== undefined)
        .map((key) => `${encodeURIComponent(key)}=${encodeURIComponent(params[key])}`)
        .join("&");
      return axios.get(queryString ? `${url}?${queryString}` : url, config);
    }
    return axios.get(url, config);
  };

  getFile = (url: string, params: any) => {
    if (params && typeof params === 'object') {
      const queryString = Object.keys(params)
        .filter(key => params[key] !== null && params[key] !== undefined)
        .map((key) => `${encodeURIComponent(key)}=${encodeURIComponent(params[key])}`)
        .join("&");
      return axios.get(queryString ? `${url}?${queryString}` : url, { responseType: "blob" });
    }
    return axios.get(url, { responseType: "blob" });
  };

  getMultiple = (urls: string[], params: any) => {
    const reqs = [];
    let queryString = "";
    if (params && typeof params === 'object') {
      queryString = Object.keys(params)
        .filter(key => params[key] !== null && params[key] !== undefined)
        .map((key) => `${encodeURIComponent(key)}=${encodeURIComponent(params[key])}`)
        .join("&");
    }

    for (const url of urls) {
      reqs.push(axios.get(queryString ? `${url}?${queryString}` : url));
    }
    return axios.all(reqs);
  };

  /**
   * post given data to url
   */
  create = (url: string, data: any, customHeaders?: any) => {
    const config = customHeaders ? { 
      headers: {
        ...axios.defaults.headers.common,
        ...customHeaders
      }
    } : undefined;
    return axios.post(url, data, config);
  };

  /**
   * Updates patch data
   */
  updatePatch = (url: string, data: any) => {
    return axios.patch(url, data);
  };

  /**
   * Updates data
   */
  update = (url: string, data: any) => {
    return axios.put(url, data);
  };

  /**
   * Deletes data
   */
  delete = (url: string) => {
    return axios.delete(url);
  };

  /**
   * post given data to url with file
   */
  createWithFile = (url: string, data: any) => {
    const formData = new FormData();
    for (const k in data) {
      formData.append(k, data[k]);
    }

    const config = {
      headers: {
        "content-type": "multipart/form-data",
      },
    };
    return axios.post(url, formData, config);
  };

  /**
   * post given data to url with file
   */
  updateWithFile = (url: string, data: any) => {
    const formData = new FormData();
    for (const k in data) {
      formData.append(k, data[k]);
    }

    const config = {
      headers: {
        "content-type": "multipart/form-data",
      },
    };
    return axios.patch(url, formData, config);
  };

  /**
   * Checks if the current user session is authenticated.
   *
   * - If there is no stored user or token, returns false.
   * - If the token is a JWT, it validates the `exp` claim.
   * - If the token is not a JWT (opaque string), it treats the
   *   presence of a non-empty token as an authenticated session.
   */
  isUserAuthenticated = () => {
    const user = this.getLoggedInUser();

    if (!user || !(user as any).token) {
      return false;
    }

    const token = (user as any).token as string;

    try {
      const decoded: any = jwtDecode(token);
      const exp = decoded?.exp;

      if (typeof exp === "number") {
        const currentTime = Date.now() / 1000;
        if (exp < currentTime) {
          console.warn("access token expired");
          clearAuthSession();
          return false;
        }
      }

      return true;
    } catch (err) {
      // Malformed token – treat as unauthenticated
      clearAuthSession();
      return false;
    }
  };

  setLoggedInUser = (session: any) => {
    if (session) {
      sessionStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session));
      logoutInProgress = false;
    } else {
      clearAuthSession();
    }
  };
  /**
   * Returns the logged in user
   */
  getLoggedInUser = () => {
    return getUserFromCookie();
  };

  setUserInSession = (modifiedUser: any) => {
    const userInfo = sessionStorage.getItem(AUTH_SESSION_KEY);
    if (userInfo) {
      const { token, user } = JSON.parse(userInfo);
      this.setLoggedInUser({ token, ...user, ...modifiedUser });
    }
  };
}

/*
Check if token available in session
*/
const user = getUserFromCookie();
if (user) {
  const { token } = user;
  if (token) {
    setAuthorization(token);
  }
}

export { APICore, setAuthorization, clearAuthSession, forceLogout };
