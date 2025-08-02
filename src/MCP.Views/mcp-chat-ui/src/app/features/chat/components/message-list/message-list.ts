import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { ChatResponseUpdateAppModelView } from '../../../../shared/models/chat-completion-view.models';

@Component({
  selector: 'app-message-list',
  imports: [CommonModule, MatCardModule, MatProgressSpinnerModule, MatIconModule],
  templateUrl: './message-list.html',
  styleUrl: './message-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageList {
  readonly messages = input.required<ChatResponseUpdateAppModelView[]>();

  // Utility methods exposed to template
  isUserMessage(message: ChatResponseUpdateAppModelView): boolean {
    return message.role === 0; // ChatRoleEnumAppModelView.User
  }


  isStreaming(message: ChatResponseUpdateAppModelView): boolean {
    return !message.finishReason;
  }

  isErrorMessage(message: ChatResponseUpdateAppModelView): boolean {
    return message.contents.some(c => c.$type === 'error');
  }

  getDisplayContent(message: ChatResponseUpdateAppModelView): string {
    return message.contents
      .filter(c => c.$type === 'text' || c.$type === 'reasoning')
      .map(c => {
        if (c.$type === 'text' || c.$type === 'reasoning') {
          return c.text || '';
        }
        return '';
      })
      .join('');
  }

  getErrorMessage(message: ChatResponseUpdateAppModelView): string {
    const errorContent = message.contents.find(c => c.$type === 'error');
    return errorContent && errorContent.$type === 'error' ? errorContent.message : '';
  }

  getMessageId(message: ChatResponseUpdateAppModelView): string {
    return message.messageId || 'unknown';
  }

  getMessageTime(message: ChatResponseUpdateAppModelView): Date {
    return message.createdAt ? new Date(message.createdAt) : new Date();
  }

}
