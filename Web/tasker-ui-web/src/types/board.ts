// Общее представление пользователя, приходящее из BoardRead (UserView)
export interface UserView {
  id: string;
  displayName: string;
  email: string;
}

export interface BoardDetails {
  id: string;
  title: string;
  description: string | null;
  ownerUserId: string;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
  columns: BoardColumn[];
  members: BoardMember[];
  labels: BoardLabel[];
  cards: BoardCard[];

  /**
   * Пользователи, связанные с доской:
   * - владелец
   * - участники
   * - авторы карточек
   * - исполнители карточек
   *
   * Может быть undefined, если старый снапшот или старая версия API.
   */
  users?: UserView[];
}

export interface BoardColumn {
  id: string;
  title: string;
  description: string | null;
  order: number;
}

export interface BoardMember {
  id: string;
  userId: string;
  role: number;
  isActive: boolean;
  joinedAt: string;
  leftAt: string | null;
}

export interface BoardLabel {
  id: string;
  title: string;
  description: string | null;
  color: string;
}

export interface BoardCard {
  id: string;
  columnId: string;
  title: string;
  description: string | null;
  order: number;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string;
  dueDate: string | null;
  assigneeUserIds: string[];
}

export interface BoardListItem {
  id: string;
  title: string;
  description: string | null;
  ownerUserId: string;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
  myRole: number;
}
