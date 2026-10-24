import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import {
  CreateTaskRequest,
  TaskItem,
  TaskService,
  UpdateTaskRequest,
} from '../../services/task.service';

@Component({
  selector: 'app-tasks',
  imports: [FormsModule, DatePipe],
  templateUrl: './tasks.html',
  styleUrl: './tasks.css',
})
export class Tasks implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  tasks = signal<TaskItem[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');
  editingId: number | null = null;
  title = '';
  description = '';
  status = 'To Do';

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.taskService.getTasks().subscribe({
      next: (data) => {
        this.tasks.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set(
          'Could not load tasks. Check that you are logged in and the API is running.',
        );
      },
    });
  }

   onSubmit(): void {
    if (this.editingId === null) {
      this.createTask();
    } else {
      this.saveEdit();
    }
  }
  private createTask(): void {
    const payload: CreateTaskRequest = {
      title: this.title,
      description: this.description,
      status: this.status,
    };
    this.taskService.createTask(payload).subscribe({
      next: () => {
        this.resetForm();
        this.loadTasks();
      },
      error: () => {
        this.errorMessage.set('Could not create the task.');
      },
    });
  }
  private saveEdit(): void {
    if (this.editingId === null) {
      return;
    }
    const payload: UpdateTaskRequest = {
      title: this.title,
      description: this.description,
      status: this.status,
    };
    this.taskService.updateTask(this.editingId, payload).subscribe({
      next: () => {
        this.resetForm();
        this.loadTasks();
      },
      error: () => {
        this.errorMessage.set('Could not update the task.');
      },
    });
  }
  startEdit(task: TaskItem): void {
    this.editingId = task.id;
    this.title = task.title;
    this.description = task.description;
    this.status = task.status;
    this.errorMessage.set('');
  }
  cancelEdit(): void {
    this.resetForm();
  }
  onDelete(task: TaskItem): void {
    const ok = confirm(`Delete task "${task.title}"?`);
    if (!ok) {
      return;
    }
    this.taskService.deleteTask(task.id).subscribe({
      next: () => {
        if (this.editingId === task.id) {
          this.resetForm();
        }
        this.loadTasks();
      },
      error: () => {
        this.errorMessage.set('Could not delete the task.');
      },
    });
  }
  private resetForm(): void {
    this.editingId = null;
    this.title = '';
    this.description = '';
    this.status = 'To Do';
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}