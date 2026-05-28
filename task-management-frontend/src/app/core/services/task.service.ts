import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateTaskRequest, TaskDto, UpdateTaskRequest } from '../models/task.models';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Tasks`;

  constructor(private http: HttpClient) {}

  getMyTasks(): Observable<TaskDto[]> {
    return this.http.get<TaskDto[]>(this.baseUrl);
  }

  getTask(id: string): Observable<TaskDto> {
    return this.http.get<TaskDto>(`${this.baseUrl}/${id}`);
  }

  createTask(request: CreateTaskRequest): Observable<TaskDto> {
    return this.http.post<TaskDto>(this.baseUrl, request);
  }

  updateTask(id: string, request: UpdateTaskRequest): Observable<TaskDto> {
    return this.http.put<TaskDto>(`${this.baseUrl}/${id}`, request);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
