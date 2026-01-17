// Backend-aligned interfaces
export enum TaskStatus {
  Incomplete = 0,
  Complete = 1,
  InProgress = 2,
  OnHold = 3,
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
}

export interface ICard {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  statusName: string;
  priority: TaskPriority;
  priorityName: string;
  userId: string;
  boardId: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface IBoard {
  id: string;
  title: string;
  cards: ICard[];
  userId: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}
