import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

interface RequireAuthProps {
  children: JSX.Element;
}

export function RequireAuth(props: RequireAuthProps) {
  const { children } = props;
  const { isAuthenticated, isInitialized } = useAuth();
  const location = useLocation();

  // Пока не знаем, есть ли сессия — просто показываем "загрузка",
  // без редиректа на /login.
  if (!isInitialized) {
    return (
      <div
        style={{
          minHeight: "100vh",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          fontFamily:
            "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
        }}
      >
        Загрузка…
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return children;
}
