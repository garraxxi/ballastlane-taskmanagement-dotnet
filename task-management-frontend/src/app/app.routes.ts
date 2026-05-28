import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./auth/auth.component').then(m => m.AuthComponent)
  },
  {
    path: 'tasks',
    loadComponent: () => import('./tasks/tasks.component').then(m => m.TasksComponent),
    canActivate: [authGuard]
  },
  {
    path: '',
    redirectTo: 'tasks',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'tasks'
  }
];
