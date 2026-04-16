import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: '',
    loadComponent: () => import('./layout').then(m => m.DefaultLayoutComponent),
    canActivate: [authGuard], // 2. On protège TOUT le layout CoreUI et ses enfants
    data: {
      title: 'Home'
    },
    children: [
      {
        path: 'dashboard',
        loadChildren: () => import('./views/dashboard/routes').then((m) => m.routes)
      },
      {
        path: 'users',
        canActivate: [roleGuard],
        data: { 
          title: 'Gestion des Utilisateurs',
          roles: ['Admin']
        },
        children: [
          {
            path: 'search',
            loadComponent: () => import('./views/users/user-search/user-search.component').then(m => m.UserSearchComponent),
            data: { title: 'Gestion utilisateurs' }
          }
        ]
      },
      {
        path: 'organisation',
        data: {
          title: 'Organisation'
        },
        loadChildren: () => import('./views/organisation/routes').then((m) => m.routes)
      },      
     
      // --- AJOUT DE LA ROUTE PROFILE ---
      {
        path: 'profile',
        loadComponent: () => import('./views/profile/profile.component').then(m => m.ProfileComponent),
        data: { title: 'Mon Profil' }
      }
    ]
  },
  {
    path: 'login',
    loadComponent: () => import('./views/pages/login/login.component').then(m => m.LoginComponent),
    data: {
      title: 'Login Page'
    }
  }, 
  { path: '**', redirectTo: 'dashboard' }
];
