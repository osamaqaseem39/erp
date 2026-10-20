import { Link } from 'react-router-dom';
import { PageMeta } from '../components/common/PageMeta';

export default function NotFound() {
  return (
    <>
      <PageMeta title="404 Not Found" />
      <div className="flex flex-col items-center justify-center min-h-screen">
        <h1 className="text-4xl font-bold mb-4">404</h1>
        <p className="text-gray-600 mb-4">Page not found</p>
        <Link 
          to="/"
          className="text-blue-600 hover:text-blue-800"
        >
          Go back home
        </Link>
      </div>
    </>
  );
} 