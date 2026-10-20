import { Route, BrowserRouter as Router, Routes } from "react-router-dom";
import AppLayout from "./layout/AppLayout";
import AuthLayout from "./layout/AuthLayout";
import ProtectedRoute from "./components/auth/ProtectedRoute";
import { useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import NotFound from './pages/NotFound';
import Login from './pages/Account/Login';
import Register from './pages/Account/Register';
// import Signup from './pages/Account/Signup'; // Uncomment if you have a Signup page

// Simple auth state handler
const AuthStateHandler = () => {
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token && location.pathname === '/signin') {
      navigate('/', { replace: true });
    }
  }, [location, navigate]);

  return null;
};

export default function App() {
  return (
    <Router>
      <AuthStateHandler />
      <Routes>
        {/* Protected Routes */}
        <Route element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
          <Route index path="/" element={<div>Dashboard</div>} />
        </Route>

        {/* Auth Routes */}
        <Route element={<AuthLayout />}>
          <Route path="/signin" element={<Login />} />
          <Route path="/signup" element={<Register />} />
        </Route>

        {/* Add this at the very bottom of your Routes */}
        <Route path="*" element={<NotFound />} />
      </Routes>
    </Router>
  );
}
