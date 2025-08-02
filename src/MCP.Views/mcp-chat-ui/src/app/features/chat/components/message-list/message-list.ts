import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { ChatResponseAppModelView } from '../../../../shared/models/chat-completion-view.models';

@Component({
  selector: 'app-message-list',
  imports: [CommonModule, MatCardModule, MatProgressSpinnerModule, MatIconModule],
  templateUrl: './message-list.html',
  styleUrl: './message-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageList {
  readonly messages = input.required<ChatResponseAppModelView[]>();

  // Utility methods exposed to template
  isUserMessage(message: ChatResponseAppModelView): boolean {
    return message.messages[0]?.role === 0; // ChatRoleEnumAppModelView.User
  }


  isStreaming(message: ChatResponseAppModelView): boolean {
    return !message.finishReason;
  }

  isErrorMessage(message: ChatResponseAppModelView): boolean {
    return message.messages[0]?.contents.some(c => c.$type === 'error') || false;
  }

  getDisplayContent(message: ChatResponseAppModelView): string {
    const msg = message.messages[0];
    if (!msg) return '';

    return message.messages
      .flatMap(m => m.contents)
      .filter(c => c.$type === 'text' || c.$type === 'reasoning')
      .map(c => {
        if (c.$type === 'text' || c.$type === 'reasoning') {
          return c.text || '';
        }
        return '';
      })
      .join('');
  }

  getErrorMessage(message: ChatResponseAppModelView): string {
    const msg = message.messages[0];
    if (!msg) return '';

    const errorContent = msg.contents.find(c => c.$type === 'error');
    return errorContent && errorContent.$type === 'error' ? errorContent.message : '';
  }

  getMessageId(message: ChatResponseAppModelView): string {
    return message.responseId || 'unknown';
  }

  getMessageTime(message: ChatResponseAppModelView): Date {
    return message.createdAt ? new Date(message.createdAt) : new Date();
  }
}
