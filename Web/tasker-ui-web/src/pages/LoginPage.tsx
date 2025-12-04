import React, { useState } from "react";
import type { FormEvent } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { login as loginRequest } from "../api/auth";
import { useAuth } from "../auth/AuthContext";

type LocationState = {
  from?: {
    pathname: string;
  };
};

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

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

    if (!email.trim() || !password.trim()) {
      setError("Введите email и пароль.");
      return;
    }

    setIsSubmitting(true);

    try {
      const result = await loginRequest({
        email: email.trim(),
        password,
      });

      login(result.userId, result.accessToken);

      navigate(from, { replace: true });
    } catch (err) {
      console.error("Не удалось выполнить вход", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось выполнить вход. Попробуйте позже.";
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
    maxWidth: "360px",
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

  return (
    <div style={pageContainerStyle}>
      <div style={formContainerStyle}>
        <h1 style={titleStyle}>Вход в Tasker</h1>
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
            <span>Пароль</span>
            <input
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              style={inputStyle}
            />
          </label>

          <button
            type="submit"
            disabled={isSubmitting}
            style={buttonStyle}
          >
            {isSubmitting ? "Входим..." : "Войти"}
          </button>

          {error && <div style={errorStyle}>{error}</div>}
        </form>
      </div>
    </div>
  );
}
