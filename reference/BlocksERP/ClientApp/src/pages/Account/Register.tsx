import React, { useState, useContext, ChangeEvent, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '../../components/ui/button/Button';
import Alert from '../../components/ui/alert/Alert';
import ComponentCard from '../../components/common/ComponentCard';
// import { ConfigContext } from '../../context/ConfigContext'; // Uncomment if you have a real context

const ConfigContext = React.createContext({
  businessName: 'Business Name',
  bgLogo: '/Content/themeimages/companylogo.png',
});

const genderOptions = [
  { value: '', label: 'Gender' },
  { value: '1', label: 'Male' },
  { value: '2', label: 'Female' },
  { value: '3', label: 'Prefer not to answer' },
];

interface RegisterForm {
  username: string;
  email: string;
  emailConfirm: string;
  password: string;
  passwordConfirm: string;
  firstname: string;
  lastname: string;
  gender: string;
  birthdate: string;
  terms: boolean;
}

const Register: React.FC = () => {
  // const { businessName, bgLogo } = useContext(ConfigContext); // Uncomment if you have a real context
  const { businessName, bgLogo } = useContext(ConfigContext); // Placeholder for now
  const [form, setForm] = useState<RegisterForm>({
    username: '',
    email: '',
    emailConfirm: '',
    password: '',
    passwordConfirm: '',
    firstname: '',
    lastname: '',
    gender: '',
    birthdate: '',
    terms: false,
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    let fieldValue: string | boolean = value;
    if (type === 'checkbox') {
      fieldValue = (e.target as HTMLInputElement).checked;
    }
    setForm(prev => ({
      ...prev,
      [name]: fieldValue,
    }));
  };

  const validate = (): string => {
    if (!form.email) return 'Please enter your email address';
    if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(form.email)) return 'Please enter a valid email address';
    if (!form.emailConfirm) return 'Please confirm your email address';
    if (form.email !== form.emailConfirm) return 'Emails do not match';
    if (!form.password) return 'Please enter your password';
    if (form.password.length < 4 || form.password.length > 20) return 'Password must be 4-20 characters';
    if (!form.passwordConfirm) return 'Please confirm your password';
    if (form.password !== form.passwordConfirm) return 'Passwords do not match';
    if (!form.terms) return 'You must agree with Terms and Conditions';
    return '';
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }
    setLoading(true);
    // TODO: Replace with actual API endpoint
    try {
      const response = await fetch('/account/register/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(form),
      });
      const data = await response.json();
      setLoading(false);
      if (data.status) {
        setSuccess('Thank you for your registration!');
        setTimeout(() => navigate('/signin'), 2000);
      } else {
        setError(data.message || 'Registration failed.');
      }
    } catch (err) {
      setLoading(false);
      setError('An error occurred. Please try again.');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900">
      <ComponentCard title={`Register for ${businessName}`} className="w-full max-w-lg shadow-lg">
        <div className="flex flex-col items-center mb-6">
          <img src={bgLogo} alt={businessName} className="h-16 mb-2" />
        </div>
        {error && <Alert variant="error" title="Registration Failed" message={error} />}
        {success && <Alert variant="success" title="Registration Successful" message={success} />}
        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="username" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Username</label>
              <input
                id="username"
                name="username"
                type="text"
                className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
                value={form.username}
                onChange={handleChange}
                autoComplete="off"
              />
            </div>
            <div>
              <label htmlFor="firstname" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">First Name</label>
              <input
                id="firstname"
                name="firstname"
                type="text"
                className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
                value={form.firstname}
                onChange={handleChange}
                autoComplete="off"
              />
            </div>
            <div>
              <label htmlFor="lastname" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Last Name</label>
              <input
                id="lastname"
                name="lastname"
                type="text"
                className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
                value={form.lastname}
                onChange={handleChange}
                autoComplete="off"
              />
            </div>
            <div>
              <label htmlFor="gender" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Gender</label>
              <select
                id="gender"
                name="gender"
                className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
                value={form.gender}
                onChange={handleChange}
              >
                {genderOptions.map(opt => (
                  <option key={opt.value} value={opt.value}>{opt.label}</option>
                ))}
              </select>
            </div>
            <div>
              <label htmlFor="birthdate" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Birthdate</label>
              <input
                id="birthdate"
                name="birthdate"
                type="text"
                placeholder="dd/mm/yyyy"
                className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
                value={form.birthdate}
                onChange={handleChange}
                autoComplete="off"
              />
            </div>
          </div>
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Email</label>
            <input
              id="email"
              name="email"
              type="email"
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
              value={form.email}
              onChange={handleChange}
              required
              autoComplete="off"
            />
          </div>
          <div>
            <label htmlFor="emailConfirm" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Confirm Email</label>
            <input
              id="emailConfirm"
              name="emailConfirm"
              type="email"
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
              value={form.emailConfirm}
              onChange={handleChange}
              required
              autoComplete="off"
            />
          </div>
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Password</label>
            <input
              id="password"
              name="password"
              type="password"
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
              value={form.password}
              onChange={handleChange}
              required
              autoComplete="off"
            />
          </div>
          <div>
            <label htmlFor="passwordConfirm" className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Confirm Password</label>
            <input
              id="passwordConfirm"
              name="passwordConfirm"
              type="password"
              className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-brand-500 dark:bg-gray-800 dark:text-white"
              value={form.passwordConfirm}
              onChange={handleChange}
              required
              autoComplete="off"
            />
          </div>
          <div className="flex items-center">
            <input
              id="terms"
              name="terms"
              type="checkbox"
              className="mr-2 rounded"
              checked={form.terms}
              onChange={handleChange}
              required
            />
            <label htmlFor="terms" className="text-sm text-gray-700 dark:text-gray-200">
              I agree with the <a href="#" className="text-brand-500 hover:underline">Terms and Conditions</a>
            </label>
          </div>
          <Button type="submit" size="md" variant="primary" disabled={loading} className="w-full">
            {loading ? <><i className="fa fa-refresh fa-spin"></i> &nbsp; Registering...</> : 'Register'}
          </Button>
        </form>
        <div className="mt-6 text-center text-sm text-gray-600 dark:text-gray-300">
          Already registered?{' '}
          <button
            type="button"
            className="text-brand-500 hover:underline"
            onClick={() => navigate('/signin')}
          >
            Sign In
          </button>
        </div>
      </ComponentCard>
    </div>
  );
};

export default Register; 