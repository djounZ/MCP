import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MessageList } from './components/message-list/message-list';
import { MessageInput } from './components/message-input/message-input';
import { Chat as ChatService } from './services/chat';

@Component({
  selector: 'app-chat',
  imports: [MatToolbarModule, MatButtonModule, MatIconModule, MatBadgeModule, MatTooltipModule, MessageList, MessageInput],
  templateUrl: './chat.html',
  styleUrl: './chat.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatComponent {
  private readonly chatService = inject(ChatService);

  protected readonly messages = this.chatService.messages;
  protected readonly isLoading = this.chatService.isLoading;

  onMessageSubmit(content: string): void {
    this.chatService.sendMessage(content);
  }

  clearChat(): void {
    this.chatService.clearChat();
  }
}
