import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ChatMessageAppModelView, AiContentAppModelUsageContentAppModelView } from '../../models/chat-completion-view.models';

@Component({
  selector: 'app-message-footer',
  templateUrl: './message-footer.html',
  styleUrl: './message-footer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule]
})
export class MessageFooterComponent {
  readonly message = input.required<ChatMessageAppModelView>();

  // Check if message has usage content or is an AI message that should show footer
  protected readonly hasUsageContent = computed(() => {
    const message = this.message();
    // Show footer for all assistant messages, not just those with usage data
    return message.role === 'assistant';
  });

  // Check if message has actual usage data
  protected readonly hasActualUsageData = computed(() => {
    return this.message().contents.some(c => c.$type === 'usage');
  });

  // Get usage content from message
  protected readonly usage = computed(() => {
    const message = this.message();
    const usageContent = message.contents.find(c => c.$type === 'usage');
    return usageContent && usageContent.$type === 'usage' ? usageContent : null;
  });

  protected readonly totalTokens = computed(() => {
    const usage = this.usage();
    if (!usage) return 0;
    return usage.details.totalTokenCount ||
      (usage.details.inputTokenCount || 0) + (usage.details.outputTokenCount || 0);
  });

  protected readonly usageText = computed(() => {
    if (!this.hasActualUsageData()) {
      return 'AI Response';
    }
    const total = this.totalTokens();
    return total ? `${total} tokens` : 'Usage tracked';
  });

  protected readonly usageTooltip = computed(() => {
    if (!this.hasActualUsageData()) {
      return 'AI assistant response - no usage data available';
    }

    const usage = this.usage();
    if (!usage) return '';

    const { inputTokenCount, outputTokenCount, totalTokenCount, additionalCounts } = usage.details;

    const input = inputTokenCount ?? 'N/A';
    const output = outputTokenCount ?? 'N/A';
    const total = totalTokenCount ?? 'N/A';

    let tooltip = `Token usage information: Input=${input}, Output=${output}, Total=${total}`;

    if (additionalCounts && typeof additionalCounts === 'object') {
      const additional = JSON.stringify(additionalCounts);
      tooltip += `, Additional=${additional}`;
    }

    return tooltip;
  });

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
