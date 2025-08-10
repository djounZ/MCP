import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ChatMessageAppModelView, AiContentAppModelUsageContentAppModelView } from '../../models/chat-completion-view.models';

@Component({
  selector: 'app-message-usage-content',
  templateUrl: './message-usage-content.html',
  styleUrl: './message-usage-content.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatTooltipModule]
})
export class MessageUsageContentComponent {
  readonly message = input.required<ChatMessageAppModelView>();



  // Check if message has actual usage data
  protected readonly hasActualUsageData = computed(() => {
    return this.message().contents.some(c => c.$type === 'usage');
  });

  protected readonly totalTokens = computed(() => {
    const message = this.message();
    const usageContent = message.contents.find(c => c.$type === 'usage') as AiContentAppModelUsageContentAppModelView | undefined;
    if (!usageContent) return 0;

    return usageContent.details.totalTokenCount ||
      (usageContent.details.inputTokenCount || 0) + (usageContent.details.outputTokenCount || 0);
  });

  protected readonly usageText = computed(() => {
    const total = this.totalTokens();
    return total ? `${total} tokens` : 'Usage tracked';
  });

  protected readonly usageTooltip = computed(() => {
    const message = this.message();
    const usageContent = message.contents.find(c => c.$type === 'usage') as AiContentAppModelUsageContentAppModelView | undefined;
    if (!usageContent) return 'No usage information available';

    const { inputTokenCount, outputTokenCount, totalTokenCount, additionalCounts } = usageContent.details;

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
}
