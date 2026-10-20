import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'https://prod.degn.life/api/v1';

const axiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      // Ensure headers object exists
      config.headers = config.headers || {};
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// // Response Interceptor
// axiosInstance.interceptors.response.use(
//   (response) => response,
//   (error) => {
//     // Only handle 401 if we have a token (we're supposed to be authenticated)
//     const token = localStorage.getItem('token');
//     if (error.response?.status === 401 && token) {
//       localStorage.removeItem('token');
//       window.location.href = '/signin';
//     }
//     return Promise.reject(error);
//   }
// );

export default axiosInstance; 