import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { ChatMessageAppModelView } from '../../../../shared/models/chat-completion-view.models';

@Component({
  selector: 'app-message-list',
  imports: [CommonModule, MatCardModule, MatProgressSpinnerModule, MatIconModule],
  templateUrl: './message-list.html',
  styleUrl: './message-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageList {
  readonly messages = input.required<ChatMessageAppModelView[]>();
  ChatRoleEnumAppModelView: any;

  // Utility methods exposed to template
  isUserMessage(message: ChatMessageAppModelView): boolean {
    return message?.role === 0; // ChatRoleEnumAppModelView.User
  }


  isStreaming(message: ChatMessageAppModelView): boolean {
    return false;
  }

  isErrorMessage(message: ChatMessageAppModelView): boolean {
    return message?.contents.some(c => c.$type === 'error') || false;
  }

  getDisplayContent(message: ChatMessageAppModelView): string {

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

  getErrorMessage(message: ChatMessageAppModelView): string {
    const errorContent = message.contents.find(c => c.$type === 'error');
    return errorContent && errorContent.$type === 'error' ? errorContent.message : '';
  }

  getMessageId(message: ChatMessageAppModelView): string {
    return 'unknown';
  }

  getMessageTime(message: ChatMessageAppModelView): Date {
    return new Date();
  }
}
