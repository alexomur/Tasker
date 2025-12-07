import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import type { BoardListItem } from "../types/board";
import { getMyBoards, createBoard, getBoardTemplates, deleteBoard } from "../api/boards";
import type { BoardTemplate } from "../api/boards";
import { useAuth } from "../auth/AuthContext";

export default function BoardsPage() {
  const [boards, setBoards] = useState<BoardListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [templates, setTemplates] = useState<BoardTemplate[]>([]);
  const [isTemplatesLoading, setIsTemplatesLoading] = useState(false);
  const [templatesError, setTemplatesError] = useState<string | null>(null);

  const [newTitle, setNewTitle] = useState("");
  const [newDescription, setNewDescription] = useState("");
  const [isCreating, setIsCreating] = useState(false);
  const [newTemplateCode, setNewTemplateCode] = useState<string>("");

  const navigate = useNavigate();
  const { logout } = useAuth();

  async function loadBoards() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await getMyBoards();
      setBoards(data);
    } catch (err) {
      console.error("Не удалось загрузить список досок", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось загрузить список досок.";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }

  async function loadTemplates() {
    setIsTemplatesLoading(true);
    setTemplatesError(null);

    try {
      const data = await getBoardTemplates();
      setTemplates(data);
    } catch (err) {
      console.error("Не удалось загрузить список шаблонов", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось загрузить список шаблонов.";
      setTemplatesError(message);
    } finally {
      setIsTemplatesLoading(false);
    }
  }

  useEffect(() => {
    void loadBoards();
    void loadTemplates();
  }, []);

  function handleOpenBoard(id: string) {
    navigate(`/boards/${id}`);
  }

  async function handleCreateBoard(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();

    const title = newTitle.trim();
    const description = newDescription.trim();

    if (!title) {
      return;
    }

    setIsCreating(true);

    try {
      await createBoard({
        title,
        description: description || null,
        templateCode: newTemplateCode || null,
      });

      setNewTitle("");
      setNewDescription("");

      await loadBoards();
    } catch (err) {
      console.error("Не удалось создать доску", err);
      const message =
        err instanceof Error ? err.message : "Не удалось создать доску.";
      alert(message);
    } finally {
      setIsCreating(false);
    }
  }

  function handleLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  async function handleDeleteBoard(id: string, title: string) {
    const ok = window.confirm(
      `Удалить доску "${title}"? Это действие необратимо.`
    );
    if (!ok) {
      return;
    }

    try {
      await deleteBoard(id);
      await loadBoards();
    } catch (err) {
      console.error("Не удалось удалить доску", err);
      const message =
        err instanceof Error ? err.message : "Не удалось удалить доску.";
      alert(message);
    }
  }

  function formatRole(role: number): string {
    switch (role) {
      case 0:
        return "Owner";
      case 1:
        return "Admin";
      case 2:
        return "Member";
      case 3:
        return "Viewer";
      default:
        return `Unknown (${role})`;
    }
  }

  const pageContainerStyle: React.CSSProperties = {
    minHeight: "100vh",
    backgroundColor: "#f4f5f7",
  };

  const pageInnerStyle: React.CSSProperties = {
    maxWidth: "1200px",
    margin: "0 auto",
    padding: "24px 16px",
    fontFamily:
      "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
  };

  const headerStyle: React.CSSProperties = {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: "24px",
  };

  const titleStyle: React.CSSProperties = {
    margin: 0,
    fontSize: "24px",
  };

  const logoutButtonStyle: React.CSSProperties = {
    padding: "6px 12px",
    borderRadius: "4px",
    border: "none",
    backgroundColor: "#d32f2f",
    color: "#ffffff",
    fontSize: "13px",
    cursor: "pointer",
  };

  const createFormContainerStyle: React.CSSProperties = {
    marginBottom: "24px",
    padding: "12px 14px",
    borderRadius: "8px",
    backgroundColor: "#ffffff",
    boxShadow: "0 1px 3px rgba(0,0,0,0.08)",
  };

  const createFormTitleStyle: React.CSSProperties = {
    margin: "0 0 8px 0",
    fontSize: "16px",
  };

  const createFormStyle: React.CSSProperties = {
    display: "flex",
    flexWrap: "wrap",
    gap: "8px",
    alignItems: "center",
  };

  const inputStyle: React.CSSProperties = {
    padding: "6px 8px",
    borderRadius: "4px",
    border: "1px solid #ccc",
    fontSize: "14px",
  };

  const createButtonStyle: React.CSSProperties = {
    padding: "6px 12px",
    borderRadius: "4px",
    border: "none",
    backgroundColor: "#0052cc",
    color: "#ffffff",
    fontSize: "14px",
    cursor: "pointer",
  };

  const listStyle: React.CSSProperties = {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))",
    gap: "16px",
  };

  const cardStyle: React.CSSProperties = {
    backgroundColor: "#ffffff",
    borderRadius: "12px",
    padding: "12px 14px",
    boxShadow: "0 1px 3px rgba(0,0,0,0.15)",
    cursor: "pointer",
    display: "flex",
    flexDirection: "column",
    gap: "6px",
    border: "1px solid #ddd",
  };

  const cardTitleStyle: React.CSSProperties = {
    margin: 0,
    fontSize: "16px",
    fontWeight: 600,
  };

  const cardDescriptionStyle: React.CSSProperties = {
    margin: 0,
    fontSize: "13px",
    opacity: 0.85,
  };

  const cardMetaStyle: React.CSSProperties = {
    marginTop: "4px",
    fontSize: "11px",
    opacity: 0.7,
    display: "flex",
    flexDirection: "column",
    gap: "2px",
  };

  const emptyStateStyle: React.CSSProperties = {
    marginTop: "24px",
    fontSize: "14px",
    opacity: 0.8,
  };

  const cardHeaderRowStyle: React.CSSProperties = {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    gap: "8px",
  };

  const cardDeleteButtonStyle: React.CSSProperties = {
    padding: "4px 8px",
    borderRadius: "4px",
    border: "none",
    backgroundColor: "#f44336",
    color: "#ffffff",
    fontSize: "11px",
    cursor: "pointer",
  };

  return (
    <div style={pageContainerStyle}>
      <div style={pageInnerStyle}>
        <header style={headerStyle}>
          <h1 style={titleStyle}>Мои доски</h1>
          <button
            type="button"
            style={logoutButtonStyle}
            onClick={handleLogout}
          >
            Выйти
          </button>
        </header>

        <section style={createFormContainerStyle}>
          <h2 style={createFormTitleStyle}>Создать доску</h2>
          <form style={createFormStyle} onSubmit={handleCreateBoard}>
            <input
              type="text"
              placeholder="Название доски"
              value={newTitle}
              onChange={(e) => setNewTitle(e.target.value)}
              style={inputStyle}
            />
            <input
              type="text"
              placeholder="Описание (необязательно)"
              value={newDescription}
              onChange={(e) => setNewDescription(e.target.value)}
              style={inputStyle}
            />
            <select
              value={newTemplateCode}
              onChange={(e) => setNewTemplateCode(e.target.value)}
              style={inputStyle}
              disabled={isTemplatesLoading}
            >
              <option value="">Пустая доска</option>
              {templates.map((t) => (
                <option key={t.code} value={t.code}>
                  {t.name}
                </option>
              ))}
            </select>
            <button
              type="submit"
              disabled={isCreating || !newTitle.trim()}
              style={createButtonStyle}
            >
              {isCreating ? "Создаём..." : "Создать доску"}
            </button>
          </form>
          {templatesError && !isTemplatesLoading && (
            <p>Не удалось загрузить шаблоны: {templatesError}</p>
          )}
        </section>

        {isLoading && <p>Загрузка…</p>}

        {!isLoading && error && <p>Ошибка: {error}</p>}

        {!isLoading && !error && boards.length === 0 && (
          <p style={emptyStateStyle}>
            У вас пока нет досок. Создайте первую через форму выше.
          </p>
        )}

        {!isLoading && !error && boards.length > 0 && (
          <div style={listStyle}>
            {boards.map((board) => (
              <div
                key={board.id}
                style={cardStyle}
                onClick={() => handleOpenBoard(board.id)}
                role="button"
              >
                <div style={cardHeaderRowStyle}>
                  <h2 style={cardTitleStyle}>{board.title}</h2>
                  <button
                    type="button"
                    style={cardDeleteButtonStyle}
                    onClick={(e) => {
                      e.stopPropagation();
                      void handleDeleteBoard(board.id, board.title);
                    }}
                  >
                    Удалить
                  </button>
                </div>

                {board.description && (
                  <p style={cardDescriptionStyle}>{board.description}</p>
                )}

                <div style={cardMetaStyle}>
                  <span>Роль: {formatRole(board.myRole)}</span>
                  <span>
                    Создана:{" "}
                    {new Date(board.createdAt).toLocaleDateString(undefined, {
                      day: "2-digit",
                      month: "2-digit",
                      year: "numeric",
                    })}
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
