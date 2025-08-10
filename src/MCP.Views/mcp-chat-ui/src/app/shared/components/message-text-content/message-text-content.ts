import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AiContentAppModelTextContentAppModelView } from '../../models/chat-completion-view.models';
import { highlightSearchMatches } from '../../utils/search.utils';

@Component({
  selector: 'app-message-text-content',
  templateUrl: './message-text-content.html',
  styleUrl: './message-text-content.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule]
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

  /**
   * Copies the text content to clipboard
   */
  protected onCopyMessage(): void {
    const text = this.content().text;
    if (text) {
      navigator.clipboard.writeText(text);
    }
  }
}
