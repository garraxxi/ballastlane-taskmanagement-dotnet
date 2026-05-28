import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TasksComponent } from './tasks.component';
import { TaskService } from '../core/services/task.service';
import { AuthService } from '../core/services/auth.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { TaskDto, TaskStatus } from '../core/models/task.models';
import { vi } from 'vitest';

describe('TasksComponent', () => {
  let component: TasksComponent;
  let fixture: ComponentFixture<TasksComponent>;
  let mockTaskService: Partial<TaskService>;
  let mockAuthService: Partial<AuthService>;
  let mockRouter: Partial<Router>;

  const mockTasks: TaskDto[] = [
    {
      id: '1',
      title: 'Test Task 1',
      description: 'Desc 1',
      status: 'Todo' as TaskStatus,
      dueDate: null,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ];

  beforeEach(async () => {
    mockTaskService = {
      getMyTasks: vi.fn().mockReturnValue(of(mockTasks)),
      createTask: vi.fn().mockReturnValue(of(mockTasks[0])),
      updateTask: vi.fn().mockReturnValue(of(mockTasks[0])),
      deleteTask: vi.fn().mockReturnValue(of(void 0)),
    };

    mockAuthService = {
      currentUser$: of(null),
      logout: vi.fn(),
    };

    mockRouter = {
      navigate: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [TasksComponent],
      providers: [
        { provide: TaskService, useValue: mockTaskService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TasksComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load tasks on init', () => {
    fixture.detectChanges(); // triggers ngOnInit

    expect(mockTaskService.getMyTasks).toHaveBeenCalled();
    expect(component.tasks()).toEqual(mockTasks);
    expect(component.loading()).toBe(false);
  });

  it('should open create modal with default due date', () => {
    component.openCreateModal();

    expect(component.showTaskModal()).toBe(true);
    expect(component.isEditMode).toBe(false);
    expect(component.taskForm.title).toBe('');
    expect(component.taskForm.dueDate).toBeTruthy(); // should have a datetime-local string
  });

  it('should call createTask when saving in create mode', () => {
    component.openCreateModal();
    component.taskForm = { title: 'New Task', description: 'Test', dueDate: null };

    component.saveTask();

    expect(mockTaskService.createTask).toHaveBeenCalledWith(
      expect.objectContaining({ title: 'New Task' })
    );
    expect(component.showTaskModal()).toBe(false);
  });

  it('should call updateTask when saving in edit mode', () => {
    const task: TaskDto = { ...mockTasks[0] };
    component.openEditModal(task);
    component.taskForm.title = 'Updated Title';

    component.saveTask();

    expect(mockTaskService.updateTask).toHaveBeenCalledWith(
      task.id,
      expect.objectContaining({ title: 'Updated Title' })
    );
  });

  it('should call updateTask when changing status', () => {
    const task: TaskDto = { ...mockTasks[0] };

    component.updateStatus(task, 'Done' as TaskStatus);

    expect(mockTaskService.updateTask).toHaveBeenCalledWith(
      task.id,
      expect.objectContaining({ status: 'Done' })
    );
  });

  it('should call deleteTask after confirmation', () => {
    const task: TaskDto = { ...mockTasks[0] };
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    component.deleteTask(task);

    expect(mockTaskService.deleteTask).toHaveBeenCalledWith(task.id);
  });

  it('should not call deleteTask if confirmation is cancelled', () => {
    const task: TaskDto = { ...mockTasks[0] };
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.deleteTask(task);

    expect(mockTaskService.deleteTask).not.toHaveBeenCalled();
  });

  it('should call logout and navigate on logout', () => {
    component.logout();

    expect(mockAuthService.logout).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });
});
