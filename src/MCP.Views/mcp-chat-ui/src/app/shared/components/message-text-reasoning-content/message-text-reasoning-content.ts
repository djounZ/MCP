import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { AiContentAppModelTextReasoningContentAppModelView } from '../../models/chat-completion-view.models';
import { highlightSearchMatches } from '../../utils/search.utils';

@Component({
    selector: 'app-message-text-reasoning-content',
    templateUrl: './message-text-reasoning-content.html',
    styleUrl: './message-text-reasoning-content.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatIconModule]
})
export class MessageTextReasoningContentComponent {
    readonly content = input.required<AiContentAppModelTextReasoningContentAppModelView>();
    readonly searchQuery = input<string>('');

    protected readonly displayContent = computed(() => {
        const text = this.content().text || '';
        const query = this.searchQuery().trim();

        if (!query) {
            return text;
        }

        return highlightSearchMatches(text, query, 'search-highlight');
    });
}
