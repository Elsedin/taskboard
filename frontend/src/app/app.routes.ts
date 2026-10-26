import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Tasks } from './pages/tasks/tasks';
import { authGuard } from './guards/auth.guard';
import { Register } from './pages/register/register';
import { guestGuard } from './guards/guest.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: Login, canActivate: [guestGuard] },
  { path: 'tasks', component: Tasks, canActivate: [authGuard] },
  { path: 'register', component: Register, canActivate: [guestGuard] },
];