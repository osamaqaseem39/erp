import { APICore } from "./apiCore";

const api = new APICore();

// account - Updated to use real API endpoints
function login(params: { username: string; password: string }) {
  const baseUrl = "/auth/login";
  return api.create(`${baseUrl}`, params);
}

function logout() {
  const baseUrl = "/auth/logout";
  return api.create(`${baseUrl}`, {});
}

function signup(params: { fullname: string; email: string; password: string }) {
  // Note: Registration is handled through userService.create() in the new implementation
  // This is kept for backward compatibility with Redux saga
  const baseUrl = "/auth/register";
  return api.create(`${baseUrl}`, params);
}

function forgotPassword(params: { username: string }) {
  const baseUrl = "/auth/forgot-password";
  return api.create(`${baseUrl}`, params);
}

export { login, logout, signup, forgotPassword };
