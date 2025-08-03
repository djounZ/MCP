import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ROUTE_PATHS } from '../../../app.routes.constants';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  imports: [MatSidenavModule, MatListModule, MatIconModule, MatButtonModule, MatTooltipModule, RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {

  protected readonly ROUTE_PATHS = ROUTE_PATHS;

}
