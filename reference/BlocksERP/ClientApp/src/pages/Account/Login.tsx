import React, { useState, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '../../components/ui/button/Button';
import Alert from '../../components/ui/alert/Alert';
import ComponentCard from '../../components/common/ComponentCard';
// import { ConfigContext } from '../../context/ConfigContext'; // Uncomment if you have a real context

const ConfigContext = React.createContext({
  businessName: 'Business Name',
  bgLogo: '/Content/themeimages/companylogo.png',
});

const Login: React.FC = () => {
  // const { businessName, bgLogo } = useContext(ConfigContext); // Uncomment if you have a real context
  const { businessName, bgLogo } = useContext(ConfigContext); // Placeholder for now
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    const formBody = new URLSearchParams();
    formBody.append('Username', username);
    formBody.append('Password', password);
    try {
      const response = await fetch('/account/login/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formBody.toString(),
      });
      const data = await response.json();
      setLoading(false);
      if (data.status) {
        window.location.href = '/home/MainMenu';
      } else {
        setError('Oops !! Incorrect Username or Password.');
      }
    } catch (err) {
      setLoading(false);
      setError('An error occurred. Please try again.');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900">
      <ComponentCard title={`Sign in to ${businessName}`} className="w-full max-w-md shadow-lg">
        <div className="flex flex-col items-center mb-6">
          <img src={bgLogo} alt={businessName} className="h-16 mb-2" />
        </div>
        {error && (
          <Alert variant="error" title="Login Failed" message={error} />
        )}
        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label htmlFor="username" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Username</label>
            <input
              id="username"
              type="text"
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
              value={username}
              onChange={e => setUsername(e.target.value)}
              required
              autoFocus
            />
          </div>
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Password</label>
            <div className="relative">
              <input
                id="password"
                type={showPassword ? 'text' : 'password'}
                className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
                value={password}
                onChange={e => setPassword(e.target.value)}
                required
              />
              <button
                type="button"
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
                onClick={() => setShowPassword(!showPassword)}
                tabIndex={-1}
                aria-label="Toggle password visibility"
              >
                <i className={`fa ${showPassword ? 'fa-eye' : 'fa-eye-slash'}`}></i>
              </button>
            </div>
          </div>
          <div className="flex items-center justify-between">
            <label className="flex items-center text-sm">
              <input
                type="checkbox"
                className="mr-2 rounded"
                checked={rememberMe}
                onChange={e => setRememberMe(e.target.checked)}
              />
              Remember me
            </label>
            <button
              type="button"
              className="text-sm text-brand-500 hover:underline"
              onClick={() => navigate('/forgot-password')}
            >
              Forgot password?
            </button>
          </div>
          <Button type="submit" size="md" variant="primary" disabled={loading} className="w-full">
            {loading ? <><i className="fa fa-refresh fa-spin"></i> &nbsp; Processing...</> : 'Sign In'}
          </Button>
        </form>
        <div className="mt-6 text-center text-sm text-gray-600 dark:text-gray-300">
          Don't have an account?{' '}
          <button
            type="button"
            className="text-brand-500 hover:underline"
            onClick={() => navigate('/signup')}
          >
            Register
          </button>
        </div>
      </ComponentCard>
    </div>
  );
};

export default Login; 