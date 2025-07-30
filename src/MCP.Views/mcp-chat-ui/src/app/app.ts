import { Component, signal } from '@angular/core';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { ChatComponent } from './features/chat/chat';
import { Sidebar } from './shared/components/sidebar/sidebar';

@Component({
  selector: 'app-root',
  imports: [MatSidenavModule, MatButtonModule, MatIconModule, MatToolbarModule, ChatComponent, Sidebar],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('MCP Chat UI');
  protected readonly sidenavOpened = signal(false);

  toggleSidenav(): void {
    this.sidenavOpened.update(opened => !opened);
  }
}
