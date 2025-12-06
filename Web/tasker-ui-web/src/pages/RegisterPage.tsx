import React, { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import {
  register as registerRequest,
  login as loginRequest,
} from "../api/auth";
import { useAuth } from "../auth/AuthContext";

type LocationState = {
  from?: {
    pathname: string;
  };
};

export default function RegisterPage() {
  const [email, setEmail] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [password, setPassword] = useState("");
  const [passwordConfirm, setPasswordConfirm] = useState("");

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();

  const from =
    (location.state as LocationState | null)?.from?.pathname ?? "/";

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);

    const trimmedEmail = email.trim();
    const trimmedDisplayName = displayName.trim();

    if (!trimmedEmail || !trimmedDisplayName || !password || !passwordConfirm) {
      setError("Заполните email, отображаемое имя и пароль.");
      return;
    }

    if (password !== passwordConfirm) {
      setError("Пароли не совпадают.");
      return;
    }

    setIsSubmitting(true);

    try {
      // 1. Регистрация
      await registerRequest({
        email: trimmedEmail,
        displayName: trimmedDisplayName,
        password,
      });

      // 2. Авто-логин
      try {
        const loginResult = await loginRequest({
          email: trimmedEmail,
          password,
        });

        login(loginResult.userId, loginResult.accessToken);
        navigate(from, { replace: true });
      } catch (loginErr) {
        console.error("Регистрация успешна, но авто-вход не удался", loginErr);
        setError(
          "Аккаунт создан, но не удалось автоматически войти. Попробуйте войти вручную."
        );
        navigate("/login", { replace: true });
      }
    } catch (err) {
      console.error("Не удалось выполнить регистрацию", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось выполнить регистрацию. Попробуйте позже.";
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  const pageContainerStyle: React.CSSProperties = {
    minHeight: "100vh",
    backgroundColor: "#f4f5f7",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    padding: "16px",
    boxSizing: "border-box",
  };

  const formContainerStyle: React.CSSProperties = {
    width: "100%",
    maxWidth: "400px",
    backgroundColor: "#ffffff",
    borderRadius: "8px",
    padding: "24px",
    boxShadow: "0 4px 12px rgba(0,0,0,0.08)",
    fontFamily:
      "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
  };

  const titleStyle: React.CSSProperties = {
    margin: "0 0 16px 0",
    fontSize: "20px",
  };

  const fieldStyle: React.CSSProperties = {
    display: "flex",
    flexDirection: "column",
    gap: "4px",
    marginBottom: "12px",
    fontSize: "14px",
  };

  const inputStyle: React.CSSProperties = {
    padding: "8px 10px",
    borderRadius: "4px",
    border: "1px solid #ccc",
    fontSize: "14px",
  };

  const buttonStyle: React.CSSProperties = {
    width: "100%",
    padding: "8px 12px",
    borderRadius: "4px",
    border: "none",
    backgroundColor: "#0052cc",
    color: "#ffffff",
    fontSize: "14px",
    cursor: "pointer",
    marginTop: "8px",
  };

  const errorStyle: React.CSSProperties = {
    color: "#b00020",
    fontSize: "13px",
    marginTop: "8px",
  };

  const footerStyle: React.CSSProperties = {
    marginTop: "12px",
    fontSize: "13px",
    textAlign: "center",
  };

  const linkButtonStyle: React.CSSProperties = {
    border: "none",
    background: "none",
    padding: 0,
    margin: 0,
    color: "#0052cc",
    cursor: "pointer",
    textDecoration: "underline",
    fontSize: "13px",
  };

  return (
    <div style={pageContainerStyle}>
      <div style={formContainerStyle}>
        <h1 style={titleStyle}>Регистрация в Tasker</h1>
        <form onSubmit={handleSubmit}>
          <label style={fieldStyle}>
            <span>Email</span>
            <input
              type="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              style={inputStyle}
            />
          </label>

          <label style={fieldStyle}>
            <span>Отображаемое имя</span>
            <input
              type="text"
              autoComplete="name"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              style={inputStyle}
            />
          </label>

          <label style={fieldStyle}>
            <span>Пароль</span>
            <input
              type="password"
              autoComplete="new-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              style={inputStyle}
            />
          </label>

          <label style={fieldStyle}>
            <span>Подтвердите пароль</span>
            <input
              type="password"
              autoComplete="new-password"
              value={passwordConfirm}
              onChange={(e) => setPasswordConfirm(e.target.value)}
              style={inputStyle}
            />
          </label>

          <button type="submit" disabled={isSubmitting} style={buttonStyle}>
            {isSubmitting ? "Регистрируем..." : "Зарегистрироваться"}
          </button>

          {error && <div style={errorStyle}>{error}</div>}

          <div style={footerStyle}>
            Уже есть аккаунт?{" "}
            <button
              type="button"
              onClick={() => navigate("/login")}
              style={linkButtonStyle}
            >
              Войти
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
