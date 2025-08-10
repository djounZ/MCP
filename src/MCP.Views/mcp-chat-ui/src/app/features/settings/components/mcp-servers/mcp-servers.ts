import { ChangeDetectionStrategy, Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { McpServerConfigHttpClient } from '../../../../core/services/mcp-server-config-http-client';
import { McpServerConfigurationItem } from '../../../../shared/models/mcp-server-config-api.models';
import { ServerFormComponent, type ServerFormData } from '../server-form/server-form';

@Component({
  selector: 'app-mcp-servers',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './mcp-servers.html',
  styleUrl: './mcp-servers.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class McpServersComponent implements OnInit {
  private readonly httpClient = inject(McpServerConfigHttpClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  protected readonly servers = signal<Map<string, McpServerConfigurationItem>>(new Map());
  protected readonly isLoading = signal(false);
  protected readonly displayedColumns = ['name', 'type', 'category', 'command', 'actions'];

  async ngOnInit(): Promise<void> {
    await this.loadServers();
  }

  async loadServers(): Promise<void> {
    this.isLoading.set(true);
    try {
      const serversMap = await this.httpClient.getAllServers();
      this.servers.set(serversMap);
    } catch (error) {
      console.error('Failed to load servers:', error);
      this.snackBar.open('Failed to load MCP servers', 'Close', { duration: 5000 });
    } finally {
      this.isLoading.set(false);
    }
  }

  async deleteServer(serverName: string): Promise<void> {
    const confirmed = confirm(`Are you sure you want to delete the server "${serverName}"?`);
    if (!confirmed) return;

    try {
      const success = await this.httpClient.deleteServer(serverName);
      if (success) {
        this.snackBar.open(`Server "${serverName}" deleted successfully`, 'Close', { duration: 3000 });
        await this.loadServers(); // Refresh the list
      } else {
        this.snackBar.open(`Failed to delete server "${serverName}"`, 'Close', { duration: 5000 });
      }
    } catch (error) {
      console.error('Error deleting server:', error);
      this.snackBar.open('An error occurred while deleting the server', 'Close', { duration: 5000 });
    }
  }

  editServer(serverName: string, config: McpServerConfigurationItem): void {
    const dialogRef = this.dialog.open(ServerFormComponent, {
      data: {
        serverName,
        config,
        isEdit: true
      } as ServerFormData,
      disableClose: true,
      maxWidth: 'none'
    });

    dialogRef.afterClosed().subscribe(async (result) => {
      if (result) {
        try {
          const updatedConfig = await this.httpClient.updateServer(result.serverName, result.config);
          if (updatedConfig) {
            this.snackBar.open(`Server "${result.serverName}" updated successfully`, 'Close', { duration: 3000 });
            await this.loadServers(); // Refresh the list
          } else {
            this.snackBar.open(`Failed to update server "${result.serverName}"`, 'Close', { duration: 5000 });
          }
        } catch (error) {
          console.error('Error updating server:', error);
          this.snackBar.open('An error occurred while updating the server', 'Close', { duration: 5000 });
        }
      }
    });
  }

  addNewServer(): void {
    const dialogRef = this.dialog.open(ServerFormComponent, {
      data: {
        isEdit: false
      } as ServerFormData,
      disableClose: true,
      maxWidth: 'none',
      maxHeight: 'none'
    });

    dialogRef.afterClosed().subscribe(async (result) => {
      if (result) {
        try {
          const createdConfig = await this.httpClient.createServer(result.serverName, result.config);
          if (createdConfig) {
            this.snackBar.open(`Server "${result.serverName}" created successfully`, 'Close', { duration: 3000 });
            await this.loadServers(); // Refresh the list
          } else {
            this.snackBar.open(`Failed to create server "${result.serverName}"`, 'Close', { duration: 5000 });
          }
        } catch (error) {
          console.error('Error creating server:', error);
          this.snackBar.open('An error occurred while creating the server', 'Close', { duration: 5000 });
        }
      }
    });
  }

  getServerEntries(): Array<[string, McpServerConfigurationItem]> {
    return Array.from(this.servers().entries());
  }

  getServerTypeIcon(type: string | null | undefined): string {
    switch (type) {
      case 'stdio':
        return 'terminal';
      case 'http':
        return 'http';
      default:
        return 'device_unknown';
    }
  }

  getConnectionColor(type: string | null | undefined): string {
    switch (type) {
      case 'stdio':
        return 'primary';
      case 'http':
        return 'accent';
      default:
        return 'warn';
    }
  }
}
