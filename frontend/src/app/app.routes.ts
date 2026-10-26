import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Tasks } from './pages/tasks/tasks';
import { authGuard } from './guards/auth.guard';
import { Register } from './pages/register/register';

export const routes: Routes = [
    {path: '', pathMatch: 'full', redirectTo: 'login'},
    {path: 'login', component:Login},
    { path: 'tasks', component: Tasks, canActivate: [authGuard] },
    { path: 'register', component: Register },
];
