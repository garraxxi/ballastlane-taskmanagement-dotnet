export type TaskStatus = 'Todo' | 'InProgress' | 'Done';

export interface TaskDto {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  dueDate: string | null;
}

export interface UpdateTaskRequest {
  title?: string | null;
  description?: string | null;
  status?: TaskStatus | null;
  dueDate?: string | null;
}
