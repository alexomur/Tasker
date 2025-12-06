import React, { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  getBoardDetails,
  addColumn,
  createCard,
  createLabel,
  updateCard,
  addBoardMember,
  moveCard,
  setCardDueDate,
  assignMemberToCard,
  unassignMemberFromCard,
  assignLabelToCard,
  unassignLabelFromCard,
} from "../api/boards";
import type {
  BoardDetails,
  BoardCard,
  BoardColumn,
  UserView,
  BoardLabel,
} from "../types/board";
import { useAuth } from "../auth/AuthContext";

export default function BoardPage() {
  const { boardId } = useParams<{ boardId: string }>();
  const { userId } = useAuth();
  const navigate = useNavigate();

  const [board, setBoard] = useState<BoardDetails | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const [newColumnTitle, setNewColumnTitle] = useState<string>("");
  const [newColumnDescription, setNewColumnDescription] =
    useState<string>("");
  const [isAddingColumn, setIsAddingColumn] = useState<boolean>(false);

  const [newLabelTitle, setNewLabelTitle] = useState<string>("");
  const [newLabelDescription, setNewLabelDescription] =
    useState<string>("");
  const [newLabelColor, setNewLabelColor] =
    useState<string>("#0079bf");
  const [isAddingLabel, setIsAddingLabel] = useState<boolean>(false);

  const [newMemberUserId, setNewMemberUserId] = useState<string>("");
  const [newMemberRole, setNewMemberRole] = useState<number>(2); // Member
  const [isAddingMember, setIsAddingMember] =
    useState<boolean>(false);
const [labelsEditorCardId, setLabelsEditorCardId] =
    useState<string | null>(null);

  // --- users: Map<userId, UserView> для быстрого доступа по id ---
  const usersById = useMemo(() => {
    const map = new Map<string, UserView>();
    if (board?.users) {
      for (const u of board.users) {
        map.set(u.id, u);
      }
    }
    return map;
  }, [board]);

  const labelsById = useMemo(() => {
    const map = new Map<string, BoardLabel>();
    if (board?.labels) {
      for (const label of board.labels) {
        map.set(label.id, label);
      }
    }
    return map;
  }, [board]);

  const labelsEditorCard = useMemo(
    () =>
      board && labelsEditorCardId
        ? board.cards.find((c) => c.id === labelsEditorCardId) ?? null
        : null,
    [board, labelsEditorCardId]
  );

  function formatUserShort(userId: string): string {
    const user = usersById.get(userId);
    if (!user) return userId;
    return user.displayName || user.email || userId;
  }

  function formatUserFull(userId: string): string {
    const user = usersById.get(userId);
    if (!user) return userId;
    if (user.displayName && user.displayName !== user.email) {
      return `${user.displayName} (${user.email})`;
    }
    return user.displayName || user.email || userId;
  }

  function getCardLabels(card: BoardCard): BoardLabel[] {
  if (!board || !card.labelIds || card.labelIds.length === 0) {
    return [];
  }

  const result: BoardLabel[] = [];
  for (const id of card.labelIds) {
      const label = labelsById.get(id);
      if (label) {
        result.push(label);
      }
    }
    return result;
  }

  async function loadBoard() {
    if (!boardId) {
      setError("Не указан идентификатор доски.");
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const data = await getBoardDetails(boardId);
      setBoard(data);
    } catch (err) {
      console.error("Ошибка загрузки доски", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось загрузить доску. Попробуйте позже.";
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadBoard();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [boardId]);

  async function handleAddColumn(e: React.FormEvent) {
    e.preventDefault();

    if (!board) {
      return;
    }

    const title = newColumnTitle.trim();
    const description = newColumnDescription.trim();

    if (!title) {
      return;
    }

    setIsAddingColumn(true);

    try {
      await addColumn(board.id, {
        title,
        description: description || null,
      });

      setNewColumnTitle("");
      setNewColumnDescription("");

      await loadBoard();
    } catch (err) {
      console.error("Не удалось добавить колонку", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось добавить колонку.";
      alert(message);
    } finally {
      setIsAddingColumn(false);
    }
  }

  async function handleAddCard(columnId: string) {
    if (!board) {
      return;
    }

    const title = window.prompt("Заголовок карточки:");
    if (!title || !title.trim()) {
      return;
    }

    const description =
      window.prompt("Описание (необязательно):") ?? "";

    try {
      await createCard(board.id, {
        columnId,
        title: title.trim(),
        createdByUserId: userId ?? board.ownerUserId,
        description: description.trim() || null,
      });

      await loadBoard();
    } catch (err) {
      console.error("Не удалось создать карточку", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось создать карточку.";
      alert(message);
    }
  }

  async function handleEditCard(card: BoardCard) {
    if (!board) {
      return;
    }

    const newTitle =
      window.prompt("Новый заголовок карточки:", card.title) ??
      card.title;

    const trimmedTitle = newTitle.trim();
    if (!trimmedTitle) {
      return;
    }

    const newDescription =
      window.prompt(
        "Новое описание (пусто — убрать описание):",
        card.description ?? ""
      ) ?? card.description ?? "";

    const descriptionTrimmed = newDescription.trim();
    const finalDescription =
      descriptionTrimmed === "" ? null : newDescription;

    try {
      await updateCard(board.id, card.id, {
        title: trimmedTitle,
        description: finalDescription,
      });

      await loadBoard();
    } catch (err) {
      console.error("Не удалось обновить карточку", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось обновить карточку.";
      alert(message);
    }
  }

  async function handleAddLabel(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();

    if (!board) {
      return;
    }

    const title = newLabelTitle.trim();
    const description = newLabelDescription.trim();
    const color = newLabelColor.trim();

    if (!title || !color) {
      return;
    }

    setIsAddingLabel(true);

    try {
      await createLabel(board.id, {
        title,
        color,
        description: description || null,
      });

      setNewLabelTitle("");
      setNewLabelDescription("");
      await loadBoard();
    } catch (err) {
      console.error("Не удалось создать метку", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось создать метку.";
      alert(message);
    } finally {
      setIsAddingLabel(false);
    }
  }

  async function handleAddMember(
    e: React.FormEvent<HTMLFormElement>
  ) {
    e.preventDefault();

    if (!board) {
      return;
    }

    const userIdValue = newMemberUserId.trim();
    if (!userIdValue) {
      return;
    }

    setIsAddingMember(true);

    try {
      await addBoardMember(board.id, {
        userId: userIdValue,
        role: newMemberRole,
      });

      setNewMemberUserId("");
      setNewMemberRole(2);

      await loadBoard();
    } catch (err) {
      console.error("Не удалось добавить участника", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось добавить участника.";
      alert(message);
    } finally {
      setIsAddingMember(false);
    }
  }

  async function handleAssignCardMember(card: BoardCard) {
    if (!board) {
      return;
    }

    const activeMembers = board.members.filter((m) => m.isActive);
    const membersHint =
      activeMembers.length > 0
        ? activeMembers
            .map(
              (m) =>
                `${formatUserFull(m.userId)} [${formatRole(m.role)}]`
            )
            .join("\n")
        : "на доске нет активных участников";

    const input = window.prompt(
      `Укажите userId участника, которого нужно назначить исполнителем.\nДоступные участники:\n${membersHint}`,
      ""
    );

    if (input === null) {
      return;
    }

    const userIdValue = input.trim();
    if (!userIdValue) {
      return;
    }

    try {
      await assignMemberToCard(board.id, card.id, {
        userId: userIdValue,
      });

      await loadBoard();
    } catch (err) {
      console.error("Не удалось назначить исполнителя", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось назначить исполнителя.";
      alert(message);
    }
  }

  async function handleUnassignCardMember(card: BoardCard) {
    if (!board) {
      return;
    }

    if (!card.assigneeUserIds || card.assigneeUserIds.length === 0) {
      alert("У карточки нет исполнителей.");
      return;
    }

    const assigneesHint = card.assigneeUserIds
      .map((id) => formatUserFull(id))
      .join("\n");

    const input = window.prompt(
      `Укажите userId исполнителя, которого нужно снять.\nТекущие исполнители:\n${assigneesHint}`,
      card.assigneeUserIds[0]
    );

    if (input === null) {
      return;
    }

    const userIdValue = input.trim();
    if (!userIdValue) {
      return;
    }

    try {
      await unassignMemberFromCard(board.id, card.id, {
        userId: userIdValue,
      });

      await loadBoard();
    } catch (err) {
      console.error("Не удалось снять исполнителя", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось снять исполнителя.";
      alert(message);
    }
  }

  async function handleMoveCard(
    card: BoardCard,
    direction: "left" | "right"
  ) {
    if (!board) {
      return;
    }

    const sortedColumns = board.columns
      .slice()
      .sort((a, b) => a.order - b.order);

    const currentIndex = sortedColumns.findIndex(
      (c) => c.id === card.columnId
    );

    if (currentIndex === -1) {
      return;
    }

    const targetIndex =
      direction === "left" ? currentIndex - 1 : currentIndex + 1;

    if (targetIndex < 0 || targetIndex >= sortedColumns.length) {
      return;
    }

    const targetColumnId = sortedColumns[targetIndex].id;

    try {
      await moveCard(board.id, card.id, {
        targetColumnId,
      });

      await loadBoard();
    } catch (err) {
      console.error("Не удалось переместить карточку", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось переместить карточку.";
      alert(message);
    }
  }

  async function handleChangeCardDueDate(card: BoardCard) {
    if (!board) {
      return;
    }

    const current = card.dueDate
      ? new Date(card.dueDate).toISOString().slice(0, 10)
      : "";

    const input = window.prompt(
      "Новый дедлайн (формат YYYY-MM-DD, пусто — убрать):",
      current
    );

    if (input === null) {
      // пользователь нажал Cancel
      return;
    }

    const trimmed = input.trim();

    let dueDate: string | null;
    if (trimmed === "") {
      dueDate = null;
    } else {
      // простейшая проверка формата YYYY-MM-DD
      const date = new Date(`${trimmed}T00:00:00`);
      if (Number.isNaN(date.getTime())) {
        alert("Некорректная дата. Ожидается формат YYYY-MM-DD.");
        return;
      }
      dueDate = date.toISOString();
    }

    try {
      await setCardDueDate(board.id, card.id, { dueDate });
      await loadBoard();
    } catch (err) {
      console.error("Не удалось изменить дедлайн", err);
      const message =
        err instanceof Error
          ? err.message
          : "Не удалось изменить дедлайн.";
      alert(message);
    }
  }

  async function handleToggleCardLabel(
  card: BoardCard,
  labelId: string,
  isChecked: boolean
) {
  if (!board) {
    return;
  }

  try {
    if (isChecked) {
      await assignLabelToCard(board.id, card.id, { labelId });
    } else {
      await unassignLabelFromCard(board.id, card.id, { labelId });
    }

    await loadBoard();
  } catch (err) {
    console.error("Не удалось обновить метки карточки", err);
    const message =
      err instanceof Error
        ? err.message
        : "Не удалось обновить метки карточки.";
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

  if (isLoading) {
    return (
      <div style={pageContainerStyle}>
        <div style={pageInnerStyle}>
          <p>Загрузка доски…</p>
        </div>
      </div>
    );
  }

  if (error || !board) {
    return (
      <div style={pageContainerStyle}>
        <div style={pageInnerStyle}>
          <h1>Ошибка</h1>
          <p>{error ?? "Доска не найдена."}</p>
          <button
            type="button"
            style={backButtonStyle}
            onClick={() => navigate("/")}
          >
            ← К моим доскам
          </button>
        </div>
      </div>
    );
  }

  const currentMember = userId
    ? board.members.find((m) => m.userId === userId && m.isActive)
    : null;

  const canManageMembers =
    !!currentMember && (currentMember.role === 0 || currentMember.role === 1);

  const canManageAssignees =
    !!currentMember &&
    (currentMember.role === 0 ||
      currentMember.role === 1 ||
      currentMember.role === 2);

  return (
    <div style={pageContainerStyle}>
      <div style={pageInnerStyle}>
        <header style={boardHeaderStyle}>
          <div>
            <button
              type="button"
              style={backButtonStyle}
              onClick={() => navigate("/")}
            >
              ← Мои доски
            </button>
            <h1 style={boardTitleStyle}>{board.title}</h1>
            {board.description && (
              <p style={boardDescriptionStyle}>{board.description}</p>
            )}
          </div>
          <div style={boardMetaStyle}>
            <span>Owner: {formatUserShort(board.ownerUserId)}</span>
            {currentMember && (
              <span>Моя роль: {formatRole(currentMember.role)}</span>
            )}
            <span>
              Создана:{" "}
              {new Date(board.createdAt).toLocaleString(undefined, {
                day: "2-digit",
                month: "2-digit",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
              })}
            </span>
          </div>
        </header>

        <section style={topSectionsWrapperStyle}>
          <section style={addColumnSectionStyle}>
            <h2 style={sectionTitleStyle}>Новая колонка</h2>
            <form style={addColumnFormStyle} onSubmit={handleAddColumn}>
              <input
                type="text"
                placeholder="Название колонки"
                value={newColumnTitle}
                onChange={(e) => setNewColumnTitle(e.target.value)}
                style={inputStyle}
              />
              <input
                type="text"
                placeholder="Описание (необязательно)"
                value={newColumnDescription}
                onChange={(e) =>
                  setNewColumnDescription(e.target.value)
                }
                style={inputStyle}
              />
              <button
                type="submit"
                disabled={isAddingColumn || !newColumnTitle.trim()}
                style={buttonStyle}
              >
                {isAddingColumn ? "Добавление..." : "Добавить колонку"}
              </button>
            </form>
          </section>

          <section style={labelsSectionStyle}>
            <h2 style={sectionTitleStyle}>Метки</h2>

            <div style={labelsListStyle}>
              {board.labels.map((label) => (
                <div key={label.id} style={labelItemStyle}>
                  <div
                    style={{
                      ...labelColorDotStyle,
                      backgroundColor: label.color,
                    }}
                  />
                  <div style={labelTextBlockStyle}>
                    <div style={labelTitleStyle}>{label.title}</div>
                    {label.description && (
                      <div style={labelDescriptionStyle}>
                        {label.description}
                      </div>
                    )}
                  </div>
                </div>
              ))}

              {board.labels.length === 0 && (
                <div style={labelsEmptyStyle}>
                  На доске пока нет меток.
                </div>
              )}
            </div>

            <form style={addLabelFormStyle} onSubmit={handleAddLabel}>
              <input
                type="text"
                placeholder="Название метки"
                value={newLabelTitle}
                onChange={(e) => setNewLabelTitle(e.target.value)}
                style={inputStyle}
              />
              <input
                type="text"
                placeholder="Описание (необязательно)"
                value={newLabelDescription}
                onChange={(e) =>
                  setNewLabelDescription(e.target.value)
                }
                style={inputStyle}
              />
              <div style={colorInputWrapperStyle}>
                <label style={colorInputLabelStyle}>
                  Цвет:
                  <input
                    type="color"
                    value={newLabelColor}
                    onChange={(e) => setNewLabelColor(e.target.value)}
                    style={colorInputStyle}
                  />
                </label>
              </div>
              <button
                type="submit"
                disabled={isAddingLabel || !newLabelTitle.trim()}
                style={buttonStyle}
              >
                {isAddingLabel ? "Добавление..." : "Добавить метку"}
              </button>
            </form>
          </section>

          <section style={membersSectionStyle}>
            <h2 style={sectionTitleStyle}>Участники</h2>
            <ul style={membersListStyle}>
              {board.members.map((m) => (
                <li key={m.id} style={memberItemStyle}>
                  <span style={memberUserStyle}>
                    {formatUserShort(m.userId)}
                  </span>
                  <span style={memberRoleStyle}>
                    {formatRole(m.role)}
                    {!m.isActive ? " (неактивен)" : ""}
                  </span>
                </li>
              ))}
            </ul>

            {canManageMembers && (
              <form
                style={addMemberFormStyle}
                onSubmit={handleAddMember}
              >
                <input
                  type="text"
                  placeholder="UserId (GUID)"
                  value={newMemberUserId}
                  onChange={(e) => setNewMemberUserId(e.target.value)}
                  style={inputStyle}
                />
                <select
                  value={newMemberRole}
                  onChange={(e) =>
                    setNewMemberRole(Number(e.target.value))
                  }
                  style={memberRoleSelectStyle}
                >
                  <option value={1}>Admin</option>
                  <option value={2}>Member</option>
                  <option value={3}>Viewer</option>
                  {/* Owner намеренно не даём, чтобы не раздавать владение направо-налево */}
                </select>
                <button
                  type="submit"
                  disabled={
                    isAddingMember || !newMemberUserId.trim()
                  }
                  style={buttonStyle}
                >
                  {isAddingMember
                    ? "Добавление..."
                    : "Добавить участника"}
                </button>
              </form>
            )}
          </section>
        </section>

        <main style={columnsWrapperStyle}>
        {board.columns
            .slice()
            .sort((a, b) => a.order - b.order)
            .map((column, index, allColumns) => (
            <ColumnView
                key={column.id}
                column={column}
                cards={board.cards.filter(
                (c) => c.columnId === column.id
                )}
                onAddCard={handleAddCard}
                onEditCard={handleEditCard}
                onMoveCardLeft={(card) =>
                handleMoveCard(card, "left")
                }
                onMoveCardRight={(card) =>
                handleMoveCard(card, "right")
                }
                onChangeCardDueDate={handleChangeCardDueDate}
                onAssignCardMember={handleAssignCardMember}
                onUnassignCardMember={handleUnassignCardMember}
                canMoveLeft={index > 0}
                canMoveRight={index < allColumns.length - 1}
                canManageAssignees={canManageAssignees}
                formatUserShort={formatUserShort}
                getCardLabels={getCardLabels}
                onEditCardLabels={(card) => setLabelsEditorCardId(card.id)}
            />
            ))}
        </main>
        {board && labelsEditorCard && (
        <CardLabelsDialog
            card={labelsEditorCard}
            boardLabels={board.labels}
            onClose={() => setLabelsEditorCardId(null)}
            onToggleLabel={handleToggleCardLabel}
        />
        )}
      </div>
    </div>
  );
}

interface ColumnViewProps {
  column: BoardColumn;
  cards: BoardCard[];
  onAddCard: (columnId: string) => void;
  onEditCard: (card: BoardCard) => void;
  onMoveCardLeft: (card: BoardCard) => void;
  onMoveCardRight: (card: BoardCard) => void;
  onChangeCardDueDate: (card: BoardCard) => void;
  onAssignCardMember: (card: BoardCard) => void;
  onUnassignCardMember: (card: BoardCard) => void;
  canMoveLeft: boolean;
  canMoveRight: boolean;
  canManageAssignees: boolean;
  formatUserShort: (userId: string) => string;

  getCardLabels: (card: BoardCard) => BoardLabel[];
  onEditCardLabels: (card: BoardCard) => void;
}

function ColumnView({
  column,
  cards,
  onAddCard,
  onEditCard,
  onMoveCardLeft,
  onMoveCardRight,
  onChangeCardDueDate,
  onAssignCardMember,
  onUnassignCardMember,
  canMoveLeft,
  canMoveRight,
  canManageAssignees,
  formatUserShort,
  getCardLabels,
  onEditCardLabels,
}: ColumnViewProps) {
  return (
    <section style={columnStyle}>
      <h2 style={columnTitleStyle}>{column.title}</h2>
      {column.description && (
        <p style={columnDescriptionStyle}>{column.description}</p>
      )}

      <div style={cardsListStyle}>
        {cards
        .slice()
        .sort((a, b) => a.order - b.order)
        .map((card) => {
            const cardLabels = getCardLabels(card);

            return (
            <article key={card.id} style={cardStyle}>
                <div style={cardTitleRowStyle}>
                <div style={cardTitleStyle}>{card.title}</div>
                <div style={cardActionsStyle}>
                    {canMoveLeft && (
                    <button
                        type="button"
                        style={cardMoveButtonStyle}
                        onClick={() => onMoveCardLeft(card)}
                        title="Переместить в колонку левее"
                    >
                        ←
                    </button>
                    )}
                    {canMoveRight && (
                    <button
                        type="button"
                        style={cardMoveButtonStyle}
                        onClick={() => onMoveCardRight(card)}
                        title="Перевести карточку дальше по пайплайну"
                    >
                        → 
                    </button>
                    )}
                    <button
                    type="button"
                    style={cardMoveButtonStyle}
                    onClick={() => onChangeCardDueDate(card)}
                    title="Изменить дедлайн карточки"
                    >
                    ⏰
                    </button>
                    {canManageAssignees && (
                    <>
                        <button
                        type="button"
                        style={cardMoveButtonStyle}
                        onClick={() => onAssignCardMember(card)}
                        title="Назначить исполнителя"
                        >
                        👤+
                        </button>
                        <button
                        type="button"
                        style={cardMoveButtonStyle}
                        onClick={() => onUnassignCardMember(card)}
                        title="Снять исполнителя"
                        >
                        👤-
                        </button>
                    </>
                    )}
                    <button
                    type="button"
                    style={cardMoveButtonStyle}
                    onClick={() => onEditCardLabels(card)}
                    title="Редактировать метки карточки"
                    >
                    🏷
                    </button>
                    <button
                    type="button"
                    style={cardEditButtonStyle}
                    onClick={() => onEditCard(card)}
                    title="Редактировать карточку"
                    >
                    ✏️
                    </button>
                </div>
                </div>

                {card.description && (
                <div style={cardDescriptionStyle}>
                    {card.description}
                </div>
                )}

                {cardLabels.length > 0 && (
                <div style={cardLabelsRowStyle}>
                    {cardLabels.map((label) => (
                    <span
                        key={label.id}
                        style={{
                        ...cardLabelPillStyle,
                        backgroundColor: label.color,
                        }}
                        title={label.description ?? ""}
                    >
                        {label.title}
                    </span>
                    ))}
                </div>
                )}

                <div style={cardMetaStyle}>
                <span>
                    Автор: {formatUserShort(card.createdByUserId)}
                </span>
                {card.assigneeUserIds.length > 0 && (
                    <span>
                    Исполнители:{" "}
                    {card.assigneeUserIds
                        .map((id) => formatUserShort(id))
                        .join(", ")}
                    </span>
                )}
                {card.dueDate && (
                    <span>
                    Дедлайн:{" "}
                    {new Date(card.dueDate).toLocaleDateString(undefined, {
                        day: "2-digit",
                        month: "2-digit",
                        year: "numeric",
                    })}
                    </span>
                )}
                </div>
            </article>
            );
        })}

        <button
          type="button"
          style={addCardButtonStyle}
          onClick={() => onAddCard(column.id)}
        >
          + Добавить карточку
        </button>
      </div>
    </section>
  );
}

interface CardLabelsDialogProps {
  card: BoardCard;
  boardLabels: BoardLabel[];
  onClose: () => void;
  onToggleLabel: (card: BoardCard, labelId: string, isChecked: boolean) => void;
}

function CardLabelsDialog({
  card,
  boardLabels,
  onClose,
  onToggleLabel,
}: CardLabelsDialogProps) {
  return (
    <div
      style={{
        position: "fixed",
        inset: 0,
        backgroundColor: "rgba(0,0,0,0.35)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        zIndex: 1000,
      }}
    >
      <div
        style={{
          backgroundColor: "#ffffff",
          borderRadius: 8,
          padding: 16,
          minWidth: 320,
          maxWidth: 480,
          maxHeight: "80vh",
          boxSizing: "border-box",
          boxShadow: "0 4px 12px rgba(0,0,0,0.2)",
          display: "flex",
          flexDirection: "column",
        }}
      >
        <h3 style={{ margin: 0, marginBottom: 8 }}>
          Метки для «{card.title}»
        </h3>

        <div
          style={{
            fontSize: 13,
            opacity: 0.8,
            marginBottom: 8,
          }}
        >
          Поставьте галочки для меток, которые должны быть на карточке.
        </div>

        <div
          style={{
            flex: 1,
            overflowY: "auto",
            paddingRight: 4,
            marginBottom: 12,
          }}
        >
          {boardLabels.length === 0 && (
            <div style={{ fontSize: 12, opacity: 0.7 }}>
              На доске ещё нет меток — создайте их в блоке «Метки» наверху.
            </div>
          )}

          {boardLabels.map((label) => {
            const checked = card.labelIds?.includes(label.id) ?? false;

            return (
              <label
                key={label.id}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 8,
                  marginBottom: 6,
                  fontSize: 13,
                  cursor: "pointer",
                }}
              >
                <input
                  type="checkbox"
                  checked={checked}
                  onChange={(e) =>
                    onToggleLabel(card, label.id, e.target.checked)
                  }
                />
                <span
                  style={{
                    width: 14,
                    height: 14,
                    borderRadius: "50%",
                    border: "1px solid rgba(0,0,0,0.2)",
                    backgroundColor: label.color,
                    flexShrink: 0,
                  }}
                />
                <span>{label.title}</span>
                {label.description && (
                  <span style={{ fontSize: 11, opacity: 0.7 }}>
                    — {label.description}
                  </span>
                )}
              </label>
            );
          })}
        </div>

        <div style={{ textAlign: "right" }}>
          <button type="button" style={buttonStyle} onClick={onClose}>
            Закрыть
          </button>
        </div>
      </div>
    </div>
  );
}

// --- styles ---

const pageContainerStyle: React.CSSProperties = {
  minHeight: "100vh",
  backgroundColor: "#f4f5f7",
};

const pageInnerStyle: React.CSSProperties = {
  maxWidth: "1200px",
  margin: "0 auto",
  padding: "24px 16px",
  fontFamily:
    "-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
};

const boardHeaderStyle: React.CSSProperties = {
  display: "flex",
  justifyContent: "space-between",
  alignItems: "flex-start",
  marginBottom: "24px",
  gap: "16px",
};

const backButtonStyle: React.CSSProperties = {
  padding: "4px 8px",
  borderRadius: "4px",
  border: "none",
  backgroundColor: "transparent",
  cursor: "pointer",
  fontSize: "13px",
  marginBottom: "4px",
};

const boardTitleStyle: React.CSSProperties = {
  margin: 0,
  fontSize: "24px",
};

const boardDescriptionStyle: React.CSSProperties = {
  marginTop: "8px",
  marginBottom: 0,
  maxWidth: "600px",
};

const boardMetaStyle: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
  gap: "4px",
  fontSize: "12px",
  opacity: 0.8,
};

const topSectionsWrapperStyle: React.CSSProperties = {
  display: "grid",
  gridTemplateColumns: "minmax(0, 2fr) minmax(0, 2fr) minmax(220px, 1fr)",
  gap: "16px",
  alignItems: "flex-start",
  marginBottom: "24px",
};

const addColumnSectionStyle: React.CSSProperties = {
  padding: "8px",
  borderRadius: "8px",
  backgroundColor: "#ffffff",
  boxShadow: "0 1px 2px rgba(0,0,0,0.05)",
};

const sectionTitleStyle: React.CSSProperties = {
  fontSize: "16px",
  margin: "0 0 8px 0",
};

const addColumnFormStyle: React.CSSProperties = {
  display: "flex",
  gap: "8px",
  alignItems: "center",
  flexWrap: "wrap",
};

const inputStyle: React.CSSProperties = {
  padding: "6px 8px",
  borderRadius: "4px",
  border: "1px solid #ccc",
  fontSize: "14px",
};

const buttonStyle: React.CSSProperties = {
  padding: "6px 12px",
  borderRadius: "4px",
  border: "none",
  backgroundColor: "#0052cc",
  color: "#ffffff",
  fontSize: "14px",
  cursor: "pointer",
};

const labelsSectionStyle: React.CSSProperties = {
  padding: "8px",
  borderRadius: "8px",
  backgroundColor: "#ffffff",
  boxShadow: "0 1px 2px rgba(0,0,0,0.05)",
};

const labelsListStyle: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
  gap: "6px",
  marginBottom: "8px",
};

const labelItemStyle: React.CSSProperties = {
  display: "flex",
  alignItems: "center",
  gap: "8px",
  fontSize: "13px",
};

const labelColorDotStyle: React.CSSProperties = {
  width: "16px",
  height: "16px",
  borderRadius: "50%",
  border: "1px solid rgba(0,0,0,0.2)",
};

const labelTextBlockStyle: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
};

const labelTitleStyle: React.CSSProperties = {
  fontWeight: 600,
};

const labelDescriptionStyle: React.CSSProperties = {
  fontSize: "12px",
  opacity: 0.8,
};

const labelsEmptyStyle: React.CSSProperties = {
  fontSize: "12px",
  opacity: 0.7,
};

const addLabelFormStyle: React.CSSProperties = {
  display: "flex",
  flexWrap: "wrap",
  gap: "8px",
  alignItems: "center",
};

const colorInputWrapperStyle: React.CSSProperties = {
  display: "flex",
  alignItems: "center",
};

const colorInputLabelStyle: React.CSSProperties = {
  fontSize: "12px",
  marginRight: "4px",
};

const colorInputStyle: React.CSSProperties = {
  padding: 0,
  marginLeft: "4px",
  border: "none",
  backgroundColor: "transparent",
  cursor: "pointer",
};

const membersSectionStyle: React.CSSProperties = {
  padding: "8px",
  borderRadius: "8px",
  backgroundColor: "#ffffff",
  boxShadow: "0 1px 2px rgba(0,0,0,0.05)",
  boxSizing: "border-box",
};

const membersListStyle: React.CSSProperties = {
  listStyleType: "none",
  padding: 0,
  margin: 0,
  display: "flex",
  flexDirection: "column",
  gap: "4px",
};

const memberItemStyle: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
  fontSize: "12px",
};

const memberUserStyle: React.CSSProperties = {
  fontWeight: 600,
};

const memberRoleStyle: React.CSSProperties = {
  opacity: 0.8,
};

const addMemberFormStyle: React.CSSProperties = {
  marginTop: "8px",
  display: "flex",
  flexWrap: "wrap",
  gap: "8px",
  alignItems: "center",
};

const memberRoleSelectStyle: React.CSSProperties = {
  padding: "6px 8px",
  borderRadius: "4px",
  border: "1px solid #ccc",
  fontSize: "14px",
};

const columnsWrapperStyle: React.CSSProperties = {
  display: "flex",
  gap: "16px",
  alignItems: "flex-start",
  overflowX: "auto",
  paddingBottom: "16px",
};

const columnStyle: React.CSSProperties = {
  minWidth: "260px",
  maxWidth: "320px",
  backgroundColor: "#ebecf0",
  borderRadius: "8px",
  padding: "8px",
  boxSizing: "border-box",
};

const columnTitleStyle: React.CSSProperties = {
  margin: "0 0 4px 0",
  fontSize: "16px",
};

const columnDescriptionStyle: React.CSSProperties = {
  margin: "0 0 8px 0",
  fontSize: "12px",
  opacity: 0.8,
};

const cardsListStyle: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
  gap: "8px",
};

const cardStyle: React.CSSProperties = {
  padding: "8px",
  borderRadius: "6px",
  backgroundColor: "#ffffff",
  boxShadow: "0 1px 2px rgba(0,0,0,0.1)",
  fontSize: "14px",
};

const cardTitleRowStyle: React.CSSProperties = {
  display: "flex",
  justifyContent: "space-between",
  alignItems: "center",
  gap: "4px",
};

const cardActionsStyle: React.CSSProperties = {
  display: "flex",
  alignItems: "center",
  gap: "4px",
};

const cardMoveButtonStyle: React.CSSProperties = {
  border: "none",
  backgroundColor: "transparent",
  cursor: "pointer",
  fontSize: "12px",
  padding: "2px 4px",
};

const cardTitleStyle: React.CSSProperties = {
  fontWeight: 600,
};

const cardEditButtonStyle: React.CSSProperties = {
  border: "none",
  backgroundColor: "transparent",
  cursor: "pointer",
  fontSize: "12px",
  padding: "2px 4px",
};

const cardDescriptionStyle: React.CSSProperties = {
  fontSize: "12px",
  marginTop: "4px",
};

const cardMetaStyle: React.CSSProperties = {
  display: "flex",
  flexDirection: "column",
  gap: "2px",
  marginTop: "6px",
  fontSize: "11px",
  opacity: 0.8,
};

const addCardButtonStyle: React.CSSProperties = {
  marginTop: "4px",
  padding: "6px 8px",
  borderRadius: "4px",
  border: "none",
  backgroundColor: "transparent",
  fontSize: "13px",
  cursor: "pointer",
  textAlign: "left",
};

const cardLabelsRowStyle: React.CSSProperties = {
  display: "flex",
  flexWrap: "wrap",
  gap: "4px",
  marginTop: "4px",
};

const cardLabelPillStyle: React.CSSProperties = {
  padding: "2px 6px",
  borderRadius: "999px",
  fontSize: "11px",
  fontWeight: 500,
  color: "#172b4d",
  backgroundColor: "#e0e0e0",
};
