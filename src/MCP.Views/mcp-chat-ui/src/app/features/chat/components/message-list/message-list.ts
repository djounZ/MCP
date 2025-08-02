import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ChatMessageAppModelView, ChatRoleEnumAppModelView } from '../../../../shared/models/chat-completion-view.models';
import { formatMessageTime } from '../../../../shared/utils/date-time.utils';

@Component({
  selector: 'app-message-list',
  imports: [CommonModule, MatCardModule, MatProgressSpinnerModule, MatIconModule, MatTooltipModule],
  templateUrl: './message-list.html',
  styleUrl: './message-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageList {
  readonly messages = input.required<ChatMessageAppModelView[]>();
  readonly isLoading = input.required<boolean>();

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

  protected getFormattedTime(messageTime: Date): string {
    return formatMessageTime(messageTime);
  }
}
