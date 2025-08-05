import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface NotifyDialogData {
  title: string;
  message: string;
  icon?: string;
}

@Component({
  selector: 'app-notify-dialog',
  imports: [MatDialogModule, MatIconModule, MatButtonModule],
  templateUrl: './notify-dialog.html',
  styleUrls: ['./notify-dialog.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotifyDialog {
  protected readonly dialogRef = inject(MatDialogRef<NotifyDialog>);
  protected readonly data = inject<NotifyDialogData>(MAT_DIALOG_DATA);

  onClose(): void {
    this.dialogRef.close();
  }
}
