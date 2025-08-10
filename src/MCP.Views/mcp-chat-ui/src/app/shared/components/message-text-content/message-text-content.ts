import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { AiContentAppModelTextContentAppModelView } from '../../models/chat-completion-view.models';
import { highlightSearchMatches } from '../../utils/search.utils';

@Component({
    selector: 'app-message-text-content',
    templateUrl: './message-text-content.html',
    styleUrl: './message-text-content.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: []
})
export class MessageTextContentComponent {
    readonly content = input.required<AiContentAppModelTextContentAppModelView>();
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
