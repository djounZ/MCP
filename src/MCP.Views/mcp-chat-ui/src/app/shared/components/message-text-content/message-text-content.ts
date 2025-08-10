import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AiContentAppModelTextContentAppModelView, ChatMessageAppModelView } from '../../models/chat-completion-view.models';
import { highlightSearchMatches } from '../../utils/search.utils';

@Component({
  selector: 'app-message-text-content',
  templateUrl: './message-text-content.html',
  styleUrl: './message-text-content.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule]
})
export class MessageTextContentComponent {

  readonly message = input.required<ChatMessageAppModelView>();
  readonly searchQuery = input<string>('');



  // Combine all text content only
  protected readonly content = computed(() => {
    const combinedText = this.message().contents
      .filter(c => c.$type === 'text')
      .map(c => {
        if (c.$type === 'text') {
          return c.text || '';
        }
        return '';
      })
      .join('');

    if (!combinedText) {
      return null;
    }

    // Create a single text content object with the combined text
    return {
      $type: 'text' as const,
      text: combinedText
    } as AiContentAppModelTextContentAppModelView;
  });

  protected readonly displayContent = computed(() => {
    const content = this.content();
    const text = content?.text ?? '';
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
    const content = this.content();
    const text = content?.text ?? '';
    if (text) {
      navigator.clipboard.writeText(text);
    }
  }
}
