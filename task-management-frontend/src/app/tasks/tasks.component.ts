import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TaskService } from '../core/services/task.service';
import { AuthService } from '../core/services/auth.service';
import { CreateTaskRequest, TaskDto, TaskStatus } from '../core/models/task.models';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.css']
})
export class TasksComponent implements OnInit {
  tasks = signal<TaskDto[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  // Modal state for Create
  showCreateModal = signal(false);
  createForm: CreateTaskRequest = {
    title: '',
    description: '',
    dueDate: null
  };
  creating = signal(false);

  get currentUser$() {
    return this.authService.currentUser$;
  }

  constructor(
    private taskService: TaskService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.loading.set(true);
    this.error.set(null);

    this.taskService.getMyTasks().subscribe({
      next: (data) => {
        this.tasks.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load tasks');
        this.loading.set(false);
        console.error(err);
      }
    });
  }

  // === Modal for Create ===
  openCreateModal() {
    const endOfDay = new Date();
    endOfDay.setHours(23, 59, 0, 0);
    const pad = (n: number) => n.toString().padStart(2, '0');
    const localStr = `${endOfDay.getFullYear()}-${pad(endOfDay.getMonth() + 1)}-${pad(endOfDay.getDate())}T${pad(endOfDay.getHours())}:${pad(endOfDay.getMinutes())}`;
    this.createForm = { title: '', description: '', dueDate: localStr };
    this.showCreateModal.set(true);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
  }

  createTask() {
    if (!this.createForm.title.trim()) return;

    this.creating.set(true);

    this.taskService.createTask(this.createForm).subscribe({
      next: () => {
        this.creating.set(false);
        this.closeCreateModal();
        this.loadTasks();
      },
      error: (err) => {
        this.creating.set(false);
        alert(err?.error?.title || 'Failed to create task');
      }
    });
  }

  // Update status inline
  updateStatus(task: TaskDto, newStatus: TaskStatus) {
    const request = {
      title: task.title,
      description: task.description,
      status: newStatus,
      dueDate: task.dueDate
    };

    this.taskService.updateTask(task.id, request).subscribe({
      next: () => this.loadTasks(),
      error: () => alert('Failed to update status')
    });
  }

  deleteTask(task: TaskDto) {
    if (!confirm(`Delete "${task.title}"?`)) return;

    this.taskService.deleteTask(task.id).subscribe({
      next: () => this.loadTasks(),
      error: () => alert('Failed to delete task')
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getStatusColor(status: TaskStatus): string {
    switch (status) {
      case 'Todo': return 'bg-gray-100 text-gray-700';
      case 'InProgress': return 'bg-blue-100 text-blue-700';
      case 'Done': return 'bg-green-100 text-green-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return 'No due date';
    return new Date(dateStr).toLocaleDateString();
  }
}
