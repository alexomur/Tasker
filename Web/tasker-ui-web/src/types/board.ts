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
