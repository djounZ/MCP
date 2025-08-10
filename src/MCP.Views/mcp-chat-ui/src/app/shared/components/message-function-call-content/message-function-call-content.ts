import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import {
    ChatMessageAppModelView,
    AiContentAppModelFunctionCallContentAppModelView
} from '../../models/chat-completion-view.models';
import { MessageContentTypeFilter } from '../../models/message-filters.models';

@Component({
    selector: 'app-message-function-call-content',
    templateUrl: './message-function-call-content.html',
    styleUrl: './message-function-call-content.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatIconModule, MatButtonModule, MatTooltipModule, MatExpansionModule]
})
export class MessageFunctionCallContentComponent {
    readonly message = input.required<ChatMessageAppModelView>();
    readonly searchQuery = input<string>('');
    readonly contentTypeFilter = input.required<MessageContentTypeFilter>();

    // Get all function call content, respect contentTypeFilter
    protected readonly functionCalls = computed(() => {
        if (!this.contentTypeFilter().functionCall) {
            return [];
        }
        return this.message().contents
            .filter(c => c.$type === 'function_call')
            .map(c => c as AiContentAppModelFunctionCallContentAppModelView);
    });

    /**
     * Copies the function call data to clipboard
     */
    protected onCopyFunctionCall(functionCall: AiContentAppModelFunctionCallContentAppModelView): void {
        const data = {
            name: functionCall.name,
            callId: functionCall.callId,
            arguments: functionCall.arguments
        };
        const text = JSON.stringify(data, null, 2);
        navigator.clipboard.writeText(text);
    }

    /**
     * Format arguments for display
     */
    protected formatArguments(args: unknown): string {
        if (!args) return '';
        return JSON.stringify(args, null, 2);
    }
}
