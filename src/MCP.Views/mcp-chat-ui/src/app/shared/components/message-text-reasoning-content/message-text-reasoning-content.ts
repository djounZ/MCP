import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AiContentAppModelTextReasoningContentAppModelView, ChatMessageAppModelView } from '../../models/chat-completion-view.models';
import { highlightSearchMatches } from '../../utils/search.utils';

@Component({
  selector: 'app-message-text-reasoning-content',
  templateUrl: './message-text-reasoning-content.html',
  styleUrl: './message-text-reasoning-content.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule]
})
export class MessageTextReasoningContentComponent {

  readonly message = input.required<ChatMessageAppModelView>();
  readonly searchQuery = input<string>('');



  // Combine all reasoning content only
  protected readonly content = computed(() => {
    const combinedReasoning = this.message().contents
      .filter(c => c.$type === 'reasoning')
      .map(c => {
        if (c.$type === 'reasoning') {
          return c.text || '';
        }
        return '';
      })
      .join('');

    if (!combinedReasoning) {
      return null;
    }

    // Create a single reasoning content object with the combined reasoning
    return {
      $type: 'reasoning' as const,
      text: combinedReasoning
    } as AiContentAppModelTextReasoningContentAppModelView;
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
   * Copies the reasoning content to clipboard
   */
  protected onCopyMessage(): void {
    const content = this.content();
    const text = content?.text ?? '';
    if (text) {
      navigator.clipboard.writeText(text);
    }
  }
}
