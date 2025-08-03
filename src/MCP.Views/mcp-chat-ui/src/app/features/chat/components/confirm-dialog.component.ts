import { Component, inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatDialogModule, MatIconModule],
  template: `
    <div class="confirm-dialog-container">
      <div class="confirm-dialog-header">
        <mat-icon color="warn" class="confirm-dialog-icon">warning</mat-icon>
        <h2 mat-dialog-title>{{ data.title || 'Confirm Action' }}</h2>
      </div>
      <mat-dialog-content>
        <p class="confirm-dialog-message">{{ data.message || 'Are you sure you want to continue?' }}</p>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-stroked-button type="button" (click)="dialogRef.close(false)">Cancel</button>
        <button mat-flat-button color="warn" mat-dialog-close="true" type="button">Confirm</button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .confirm-dialog-container {
      padding: 24px 16px 16px 16px;
      border-radius: 16px;
      min-width: 320px;
      background: var(--mat-dialog-background, #fff);
    }
    .confirm-dialog-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 8px;
    }
    .confirm-dialog-icon {
      font-size: 36px;
      color: #f44336;
      margin-right: 4px;
      flex-shrink: 0;
      vertical-align: middle;
      overflow: visible;
    }
    h2[mat-dialog-title] {
      margin-left: 2px;
      font-weight: 500;
      font-size: 1.25rem;
    }
    .confirm-dialog-message {
      margin: 0 0 16px 0;
      font-size: 1rem;
      color: #333;
    }
    mat-dialog-actions {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
    }
  `]
})
export class ConfirmDialogComponent {
  dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  data = (inject(MAT_DIALOG_DATA) ?? {}) as { title?: string; message?: string };
}
