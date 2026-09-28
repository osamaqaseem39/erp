import config from "../config";

const Footer = () => {
  return (
    <footer className="footer h-16 flex items-center px-6 bg-white shadow dark:bg-gray-800">
      <div className="flex md:justify-between justify-center w-full gap-4">
        <div>
          {new Date().getFullYear()} © {config.APP_NAME}
        </div>
      </div>
    </footer>
  )
}

export default Footer
