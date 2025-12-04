import { httpClient } from "./client";
import type { BoardDetails, BoardListItem } from "../types/board";

export function getBoardDetails(boardId: string): Promise<BoardDetails> {
  return httpClient.get<BoardDetails>(`/api/v1/boards/${boardId}`);
}

export function getMyBoards(): Promise<BoardListItem[]> {
  return httpClient.get<BoardListItem[]>("/api/v1/boards?mine=true");
}

export interface AddColumnPayload {
  title: string;
  description?: string | null;
}

export interface AddColumnResult {
  columnId: string;
  title: string;
  description: string | null;
  order: number;
}

export function addColumn(
  boardId: string,
  payload: AddColumnPayload
): Promise<AddColumnResult> {
  return httpClient.post<AddColumnPayload, AddColumnResult>(
    `/api/v1/boards/${boardId}/columns`,
    payload
  );
}

export interface CreateCardPayload {
  columnId: string;
  title: string;
  createdByUserId: string;
  description?: string | null;
  dueDate?: string | null;
}

export interface CreateCardResult {
  cardId: string;
  columnId: string;
  order: number;
}

export function createCard(
  boardId: string,
  payload: CreateCardPayload
): Promise<CreateCardResult> {
  return httpClient.post<CreateCardPayload, CreateCardResult>(
    `/api/v1/boards/${boardId}/cards`,
    payload
  );
}

export interface UpdateCardPayload {
  title: string;
  description?: string | null;
}

export interface UpdateCardResult {
  cardId: string;
}

export function updateCard(
  boardId: string,
  cardId: string,
  payload: UpdateCardPayload
): Promise<UpdateCardResult> {
  return httpClient.put<UpdateCardPayload, UpdateCardResult>(
    `/api/v1/boards/${boardId}/cards/${cardId}`,
    payload
  );
}

export interface SetCardDueDatePayload {
  dueDate: string | null; // ISO-строка или null
}

export interface SetCardDueDateResult {
  cardId: string;
  dueDate: string | null;
}

/**
 * Устанавливает или сбрасывает дедлайн карточки.
 * POST /api/v1/boards/{boardId}/cards/{cardId}/due-date
 */
export function setCardDueDate(
  boardId: string,
  cardId: string,
  payload: SetCardDueDatePayload
): Promise<SetCardDueDateResult> {
  return httpClient.post<SetCardDueDatePayload, SetCardDueDateResult>(
    `/api/v1/boards/${boardId}/cards/${cardId}/due-date`,
    payload
  );
}

export interface AddBoardMemberPayload {
  userId: string;
  role: number; // BoardMemberRole: 0=Owner,1=Admin,2=Member,3=Viewer
}

/**
 * Добавляет участника на доску: POST /api/v1/boards/{boardId}/members.
 * Результат нам для MVP не важен, поэтому игнорируем тело ответа.
 */
export function addBoardMember(
  boardId: string,
  payload: AddBoardMemberPayload
): Promise<void> {
  return httpClient
    .post<AddBoardMemberPayload, unknown>(
      `/api/v1/boards/${boardId}/members`,
      payload
    )
    .then(() => {});
}

export interface CreateBoardPayload {
  title: string;
  description?: string | null;
}

export interface CreateBoardResult {
  boardId: string;
}

/**
 * Создаёт доску через BoardWrite: POST /api/v1/boards.
 */
export function createBoard(
  payload: CreateBoardPayload
): Promise<CreateBoardResult> {
  return httpClient.post<CreateBoardPayload, CreateBoardResult>(
    "/api/v1/boards",
    payload
  );
}

export interface CreateLabelPayload {
  title: string;
  color: string;
  description?: string | null;
}

export interface CreateLabelResult {
  labelId: string;
  title: string;
  color: string;
  description: string | null;
}

/**
 * Добавляет метку на доску: POST /api/v1/boards/{boardId}/labels.
 */
export function createLabel(
  boardId: string,
  payload: CreateLabelPayload
): Promise<CreateLabelResult> {
  return httpClient.post<CreateLabelPayload, CreateLabelResult>(
    `/api/v1/boards/${boardId}/labels`,
    payload
  );
}

export interface MoveCardPayload {
  targetColumnId: string;
}

export interface MoveCardResult {
  cardId: string;
  columnId: string;
  order: number;
}

/**
 * Перемещает карточку в другую колонку:
 * POST /api/v1/boards/{boardId}/cards/{cardId}/move
 */
export function moveCard(
  boardId: string,
  cardId: string,
  payload: MoveCardPayload
): Promise<MoveCardResult> {
  return httpClient.post<MoveCardPayload, MoveCardResult>(
    `/api/v1/boards/${boardId}/cards/${cardId}/move`,
    payload
  );
}

/**
 * Назначение / снятие исполнителя по карточке.
 * POST /api/v1/boards/{boardId}/cards/{cardId}/assignees
 * POST /api/v1/boards/{boardId}/cards/{cardId}/assignees/remove
 */

export interface CardAssigneePayload {
  userId: string;
}

export interface AssignMemberToCardResult {
  cardId: string;
  userId: string;
}

export interface UnassignMemberFromCardResult {
  cardId: string;
  userId: string;
}

export function assignMemberToCard(
  boardId: string,
  cardId: string,
  payload: CardAssigneePayload
): Promise<AssignMemberToCardResult> {
  return httpClient.post<CardAssigneePayload, AssignMemberToCardResult>(
    `/api/v1/boards/${boardId}/cards/${cardId}/assignees`,
    payload
  );
}

export function unassignMemberFromCard(
  boardId: string,
  cardId: string,
  payload: CardAssigneePayload
): Promise<UnassignMemberFromCardResult> {
  return httpClient.post<CardAssigneePayload, UnassignMemberFromCardResult>(
    `/api/v1/boards/${boardId}/cards/${cardId}/assignees/remove`,
    payload
  );
}
