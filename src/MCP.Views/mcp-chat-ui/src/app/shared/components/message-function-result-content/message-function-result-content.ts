import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import {
    ChatMessageAppModelView,
    AiContentAppModelFunctionResultContentAppModelView
} from '../../models/chat-completion-view.models';
import { MessageContentTypeFilter } from '../../models/message-filters.models';

@Component({
    selector: 'app-message-function-result-content',
    templateUrl: './message-function-result-content.html',
    styleUrl: './message-function-result-content.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatIconModule, MatButtonModule, MatTooltipModule, MatExpansionModule]
})
export class MessageFunctionResultContentComponent {
    readonly message = input.required<ChatMessageAppModelView>();
    readonly searchQuery = input<string>('');
    readonly contentTypeFilter = input.required<MessageContentTypeFilter>();

    // Get all function result content, respect contentTypeFilter
    protected readonly functionResults = computed(() => {
        if (!this.contentTypeFilter().functionResult) {
            return [];
        }
        return this.message().contents
            .filter(c => c.$type === 'function_result')
            .map(c => c as AiContentAppModelFunctionResultContentAppModelView);
    });

    /**
     * Copies the function result data to clipboard
     */
    protected onCopyFunctionResult(functionResult: AiContentAppModelFunctionResultContentAppModelView): void {
        const data = {
            callId: functionResult.callId,
            result: functionResult.result
        };
        const text = JSON.stringify(data, null, 2);
        navigator.clipboard.writeText(text);
    }

    /**
     * Format result for display
     */
    protected formatResult(result: unknown): string {
        if (result === null || result === undefined) return 'null';
        if (typeof result === 'string') return result;
        return JSON.stringify(result, null, 2);
    }
}
