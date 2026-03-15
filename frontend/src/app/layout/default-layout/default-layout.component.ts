import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NgScrollbar } from 'ngx-scrollbar';

import { IconDirective } from '@coreui/icons-angular';
import {
  ContainerComponent,
  ShadowOnScrollDirective,
  SidebarBrandComponent,
  SidebarComponent,
  SidebarFooterComponent,
  SidebarHeaderComponent,
  SidebarNavComponent,
  SidebarToggleDirective,
  SidebarTogglerDirective,
  INavData,
} from '@coreui/angular';

import { DefaultFooterComponent, DefaultHeaderComponent } from './';
import { navItems } from './_nav';
import { AuthService } from '../../services/user/auth.service';

function isOverflown(element: HTMLElement) {
  return (
    element.scrollHeight > element.clientHeight ||
    element.scrollWidth > element.clientWidth
  );
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './default-layout.component.html',
  styleUrls: ['./default-layout.component.scss'],
  imports: [
    SidebarComponent,
    SidebarHeaderComponent,
    SidebarBrandComponent,
    SidebarNavComponent,
    SidebarFooterComponent,
    SidebarToggleDirective,
    SidebarTogglerDirective,
    ContainerComponent,
    DefaultFooterComponent,
    DefaultHeaderComponent,
   /* IconDirective,*/
    NgScrollbar,
    RouterOutlet,
    RouterLink,
    ShadowOnScrollDirective
  ]
})
export class DefaultLayoutComponent {
  public navItems = [...navItems];
  // On crée une variable vide qui contiendra uniquement les items autorisés
  public filteredNavItems: INavData[] = [];

  authService = inject(AuthService);
  ngOnInit(): void {
    const currentRole = this.authService.getUserRole();

    // On filtre le tableau original
    this.filteredNavItems = navItems.filter(item => {
      // Si l'item a un attribut role défini à 'Admin'
      if (item.attributes && item.attributes['role'] === 'Admin') {
        // On ne le garde que si l'utilisateur est Admin
        return currentRole === 'Admin';
      }
      // Sinon, c'est un item public (Dashboard, etc.), on le garde toujours
      return true;
    });
  }
}
