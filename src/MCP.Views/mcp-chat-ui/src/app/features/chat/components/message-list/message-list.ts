import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ChatMessageAppModelView, AiContentAppModelUsageContentAppModelView } from '../../../../shared/models/chat-completion-view.models';
import { formatMessageTime } from '../../../../shared/utils/date-time.utils';

@Component({
  selector: 'app-message-list',
  imports: [CommonModule, MatCardModule, MatProgressSpinnerModule, MatIconModule, MatTooltipModule],
  templateUrl: './message-list.html',
  styleUrl: './message-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageList {
  /**
   * Returns a plain text tooltip for token usage info
   */
  protected getUsageTooltip(usage: any): string {
    if (!usage || !usage.details) return '';
    const input = usage.details.inputTokenCount ?? 'N/A';
    const output = usage.details.outputTokenCount ?? 'N/A';
    const total = usage.details.totalTokenCount ?? 'N/A';
    let tooltip = `Token usage information: Input=${input}, Output=${output}, Total=${total}`;
    if (usage.details.additionalCounts && typeof usage.details.additionalCounts === 'object') {
      const additional = JSON.stringify(usage.details.additionalCounts);
      tooltip += `, Additional=${additional}`;
    }
    return tooltip;
  }
  readonly messages = input.required<ChatMessageAppModelView[]>();
  readonly isLoading = input.required<boolean>();

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

  hasUsageContent(message: ChatMessageAppModelView): boolean {
    return message.contents.some(c => c.$type === 'usage');
  }

  getUsageContent(message: ChatMessageAppModelView): AiContentAppModelUsageContentAppModelView | null {
    const usageContent = message.contents.find(c => c.$type === 'usage');
    return usageContent && usageContent.$type === 'usage' ? usageContent : null;
  }

  protected getFormattedTime(messageTime: Date): string {
    return formatMessageTime(messageTime);
  }
}
