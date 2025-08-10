import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ChatMessageAppModelView } from '../../models/chat-completion-view.models';
import { highlightSearchMatches } from '../../utils/search.utils';

@Component({
    selector: 'app-message-content-user',
    templateUrl: './message-content-user.html',
    styleUrl: './message-content-user.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatIconModule, MatProgressSpinnerModule]
})
export class MessageContentUserComponent {
    readonly message = input.required<ChatMessageAppModelView>();
    readonly searchQuery = input<string>('');

    protected readonly isError = computed(() =>
        this.message().contents.some(c => c.$type === 'error')
    );

    protected readonly errorMessage = computed(() => {
        const errorContent = this.message().contents.find(c => c.$type === 'error');
        return errorContent && errorContent.$type === 'error' ? errorContent.message : '';
    });

    protected readonly defaultDisplayContent = computed(() =>
        this.message().contents
            .filter(c => c.$type === 'text' || c.$type === 'reasoning')
            .map(c => {
                if (c.$type === 'text' || c.$type === 'reasoning') {
                    return c.text || '';
                }
                return '';
            })
            .join('')
    );

    protected readonly displayContentWithHighlight = computed(() => {
        const content = this.defaultDisplayContent();
        const query = this.searchQuery().trim();

        if (!query) {
            return content;
        }

        return highlightSearchMatches(content, query, 'search-highlight');
    });
}
