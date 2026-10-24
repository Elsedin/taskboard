import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TaskItem {
  id: number;
  title: string;
  description: string;
  status: string;
  createdAt: string;
  userId: number;
}

@Injectable({
  providedIn: 'root',
})
export class TaskService {
  private readonly http = inject(HttpClient);

  getTasks(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${environment.apiUrl}/api/tasks`);
  }

  createTask(payload: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${environment.apiUrl}/api/tasks`, payload);
  }

  updateTask(id: number, payload: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${environment.apiUrl}/api/tasks/${id}`, payload);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/api/tasks/${id}`);
  }
}


export interface CreateTaskRequest {
  title: string;
  description: string;
  status: string;
}

export interface UpdateTaskRequest {
  title: string;
  description: string;
  status: string;
}