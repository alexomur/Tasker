import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";

interface AuthContextValue {
  userId: string | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isInitialized: boolean;
  login: (userId: string, accessToken: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider(props: { children: React.ReactNode }) {
  const { children } = props;

  const [userId, setUserId] = useState<string | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isInitialized, setIsInitialized] = useState(false);

  // Инициализация из localStorage при загрузке страницы
  useEffect(() => {
    try {
      const storedToken = localStorage.getItem("accessToken");
      const storedUserId = localStorage.getItem("userId");

      if (storedToken && storedUserId) {
        setAccessToken(storedToken);
        setUserId(storedUserId);
      }
    } finally {
      // В любом случае помечаем, что инициализацию завершили
      setIsInitialized(true);
    }
  }, []);

  const value: AuthContextValue = useMemo(
    () => ({
      userId,
      accessToken,
      isAuthenticated: !!userId && !!accessToken,
      isInitialized,
      login: (newUserId, newToken) => {
        setUserId(newUserId);
        setAccessToken(newToken);
        localStorage.setItem("userId", newUserId);
        localStorage.setItem("accessToken", newToken);
      },
      logout: () => {
        setUserId(null);
        setAccessToken(null);
        localStorage.removeItem("userId");
        localStorage.removeItem("accessToken");
      },
    }),
    [userId, accessToken, isInitialized]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
