import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CreateTaskRequest, TaskItem, TaskService } from '../../services/task.service';

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

  onCreate(): void {
    const payload: CreateTaskRequest = {
      title: this.title,
      description: this.description,
      status: this.status,
    };

    this.taskService.createTask(payload).subscribe({
      next: () => {
        this.title = '';
        this.description = '';
        this.status = 'To Do';
        this.loadTasks();
      },
      error: () => {
        this.errorMessage.set('Could not create the task.');
      },
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}