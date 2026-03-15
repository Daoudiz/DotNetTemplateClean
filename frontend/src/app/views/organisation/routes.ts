import { Routes } from '@angular/router';
import { roleGuard } from '../../guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    data: { title: 'Organisation' },
    children: [
      {
        path: '',
        redirectTo: 'entities/search', // Redirige /organisation vers la recherche d'entités
        pathMatch: 'full'
      },
      {
        path: 'entities/search',
        loadComponent: () => import('./entite-search/entite-search.component').then(m => m.EntiteSearchComponent),
        canActivate: [roleGuard],
        data: {
          title: 'Gestion des entités',
          roles: ['Admin']
        }
      }
    ]
  }
];