import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: 'home',
    loadComponent: () => import('./features/home/home').then(m => m.HomeComponent)
  },
  {
    path: 'chat',
    loadComponent: () => import('./features/chat/chat').then(m => m.ChatComponent)
  }
  ,
  {
    path: 'dev-in-progress',
    loadComponent: () => import('./features/dev-in-progress/dev-in-progress').then(m => m.DevInProgressComponent)
  }
];
