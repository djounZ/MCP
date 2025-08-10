import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ChatMessageAppModelView, AiContentAppModelUsageContentAppModelView } from '../../models/chat-completion-view.models';
import { MessageUsageContentComponent } from '../message-usage-content/message-usage-content';

@Component({
  selector: 'app-message-footer',
  templateUrl: './message-footer.html',
  styleUrl: './message-footer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule, MessageUsageContentComponent]
})
export class MessageFooterComponent {
  readonly message = input.required<ChatMessageAppModelView>();


  /**
   * Copies the raw message content to clipboard
   */
  protected onCopyMessage(): void {
    const message = this.message();
    if (message?.contents) {
      // Concatenate all text and reasoning contents
      const rawContent = message.contents
        .filter(c => c.$type === 'text' || c.$type === 'reasoning')
        .map(c => c.text || '')
        .join('');
      if (rawContent) {
        navigator.clipboard.writeText(rawContent);
      }
    }
  }
}
