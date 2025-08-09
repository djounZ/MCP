// ...existing code...
import { ChangeDetectionStrategy, Component, output, signal, input, inject } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { TextFieldModule } from '@angular/cdk/text-field';
import { ToolConfigurationDialog } from '../tool-configuration-dialog/tool-configuration-dialog';
import { AiToolAppModelView } from '../../models/chat-completion-view.models';

@Component({
  selector: 'app-message-input',
  imports: [MatInputModule, MatButtonModule, MatIconModule, MatTooltipModule, FormsModule, TextFieldModule],
  templateUrl: './message-input.html',
  styleUrl: './message-input.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageInput {
  private readonly dialog = inject(MatDialog);

  public focusInput(): void {
    const textarea = document.querySelector('textarea[matInput]') as HTMLTextAreaElement;
    if (textarea) {
      textarea.focus();
    }
  }
  /**
   * Stores history of user-sent messages
   */
  private messageHistory: string[] = [];
  private historyIndex: number = -1;
  private readonly HISTORY_LIMIT = 50;
  protected readonly messageContent = signal('');
  readonly messageSubmit = output<string>();
  readonly toolsConfigured = output<Map<string, AiToolAppModelView[]> | null>();
  readonly disabled = input<boolean>(false);

  configureTools(): void {
    const dialogRef = this.dialog.open(ToolConfigurationDialog, {
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result !== null && result !== undefined) {
        // result is Map<string, AiToolAppModelView[]> | null
        this.toolsConfigured.emit(result);
      }
    });
  }

  sendMessage(): void {
    const content = this.messageContent().trim();
    if (content) {
      this.messageSubmit.emit(content);
      // Add to history if not duplicate of last
      if (this.messageHistory.length === 0 || this.messageHistory[this.messageHistory.length - 1] !== content) {
        this.messageHistory.push(content);
        if (this.messageHistory.length > this.HISTORY_LIMIT) {
          this.messageHistory.shift();
        }
      }
      this.messageContent.set('');
      this.historyIndex = -1;
    }
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
      return;
    }
    // Up arrow: previous message in history
    if (event.key === 'ArrowUp') {
      const textarea = event.target as HTMLTextAreaElement;
      if (this.messageHistory.length > 0) {
        if (this.historyIndex === -1) {
          this.historyIndex = this.messageHistory.length - 1;
        } else if (this.historyIndex > 0) {
          this.historyIndex--;
        }
        this.messageContent.set(this.messageHistory[this.historyIndex]);
        setTimeout(() => {
          textarea.selectionStart = textarea.selectionEnd = this.messageContent().length;
        });
      }
      event.preventDefault();
      return;
    }
    // Down arrow: next message or clear
    if (event.key === 'ArrowDown') {
      const textarea = event.target as HTMLTextAreaElement;
      if (this.messageHistory.length > 0 && this.historyIndex !== -1) {
        if (this.historyIndex < this.messageHistory.length - 1) {
          this.historyIndex++;
          this.messageContent.set(this.messageHistory[this.historyIndex]);
        } else {
          this.historyIndex = -1;
          this.messageContent.set('');
        }
        setTimeout(() => {
          textarea.selectionStart = textarea.selectionEnd = this.messageContent().length;
        });
      }
      event.preventDefault();
      return;
    }
    // Reset history index if user types
    if (event.key.length === 1) {
      this.historyIndex = -1;
    }
  }
}
