import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AiContentAppModelUsageContentAppModelView } from '../../models/chat-completion-view.models';

@Component({
    selector: 'app-message-usage',
    templateUrl: './message-usage.html',
    styleUrl: './message-usage.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatIconModule, MatButtonModule, MatTooltipModule]
})
export class MessageUsageComponent {
    readonly usage = input.required<AiContentAppModelUsageContentAppModelView>();
    readonly copyMessage = output<void>();

    protected readonly totalTokens = computed(() => {
        const usage = this.usage();
        return usage.details.totalTokenCount ||
            (usage.details.inputTokenCount || 0) + (usage.details.outputTokenCount || 0);
    });

    protected readonly usageText = computed(() => {
        const total = this.totalTokens();
        return total ? `${total} tokens` : 'Usage tracked';
    });

    protected readonly usageTooltip = computed(() => {
        const { inputTokenCount, outputTokenCount, totalTokenCount, additionalCounts } = this.usage().details;

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

    protected onCopyMessage(): void {
        this.copyMessage.emit();
    }
}
