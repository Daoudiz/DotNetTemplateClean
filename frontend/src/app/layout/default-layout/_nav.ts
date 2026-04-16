import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  {
    name: 'Dashboard',
    url: '/dashboard',
    iconComponent: { name: 'cil-speedometer' }
  },
  {
    title: true,
    name: 'Referentiel',
    class: 'mt-auto',
    attributes: { role: 'Admin' }
  },
  {
    name: 'Organisation',
    url: '/organisation',
    iconComponent: { name: 'cil-building' },
    attributes: { role: 'Admin' },
    children: [
      {
        name: 'Entites',
        url: '/organisation/entities/search',
        icon: 'nav-icon-bullet'
      },
      {
        name: 'Personnel',
        url: '/organisation/personnel/search',
        icon: 'nav-icon-bullet'
      }
    ]
  },
  {
    name: 'Utilisateurs',
    url: '/users/search',
    iconComponent: { name: 'cil-people' },
    attributes: { role: 'Admin' }
  }
];
