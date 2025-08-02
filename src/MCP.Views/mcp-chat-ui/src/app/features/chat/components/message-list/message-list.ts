import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { ChatResponseUpdateAppModelView } from '../../../../shared/models/chat-completion-view.models';
import {
  isUserMessage,
  isStreaming,
  getDisplayContent,
  isErrorMessage,
  getMessageId,
  getMessageTime
} from '../../services/chat';

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
  isUserMessage = isUserMessage;
  isStreaming = isStreaming;
  getDisplayContent = getDisplayContent;
  isErrorMessage = isErrorMessage;
  getMessageId = getMessageId;
  getMessageTime = getMessageTime;
}
