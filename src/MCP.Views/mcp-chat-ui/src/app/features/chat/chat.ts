import { ChangeDetectionStrategy, Component, inject, signal, ViewChild, effect } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MessageList } from '../../shared/components/message-list/message-list';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmationDialog, ConfirmationDialogData } from '../../shared/components/confirmation-dialog/confirmation-dialog';
import { ChatOptionsComponent } from './components/chat-options/chat-options';
import { Chat as ChatService } from './services/chat';
import { MessageInput } from '../../shared/components/message-input/message-input';

@Component({
  selector: 'app-chat',
  imports: [MatToolbarModule, MatButtonModule, MatIconModule, MatBadgeModule, MatTooltipModule, MatSidenavModule, MatDialogModule, MessageList, MessageInput, ChatOptionsComponent],
  templateUrl: './chat.html',
  styleUrl: './chat.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatComponent {
  constructor(private dialog: MatDialog) { }

  confirmClearChat(): void {
    const dialogRef = this.dialog.open<ConfirmationDialog, ConfirmationDialogData, boolean>(ConfirmationDialog, {
      data: {
        title: 'Clear Chat',
        message: 'Are you sure you want to clear the chat?'
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.clearChat();
      }
    });
  }
  @ViewChild(MessageInput) messageInputComponent?: MessageInput;

  private readonly _focusEffect = effect(() => {
    if (!this.isLoading() && this.messageInputComponent) {
      setTimeout(() => {
        this.messageInputComponent?.focusInput();
      });
    }
  });
  protected readonly chatService = inject(ChatService);

  protected readonly messages = this.chatService.messages;
  protected readonly isLoading = this.chatService.isLoading;
  protected readonly showOptions = signal(false);

  onMessageSubmit(content: string): void {
    this.chatService.sendMessage(content);
  }

  clearChat(): void {
    this.chatService.clearChat();
  }

  exportChat(): void {
    this.chatService.exportChat();
  }

  toggleOptions(): void {
    this.showOptions.update(show => !show);
  }

  onOptionsChanged(): void {
    // Options have been updated, could show a toast notification
    console.log('Chat options updated');
  }
}
