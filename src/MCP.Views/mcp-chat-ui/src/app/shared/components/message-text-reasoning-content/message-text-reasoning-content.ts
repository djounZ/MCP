import { ChangeDetectionStrategy, Component, input, computed, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MarkdownComponent } from 'ngx-markdown';
import { AiContentAppModelTextReasoningContentAppModelView, ChatMessageAppModelView } from '../../models/chat-completion-view.models';
import { MessageContentTypeFilter } from '../../models/message-filters.models';
import { highlightSearchMatches } from '../../utils/search.utils';
import { isMarkdownContent } from '../../utils/string.utils';

@Component({
  selector: 'app-message-text-reasoning-content',
  templateUrl: './message-text-reasoning-content.html',
  styleUrl: './message-text-reasoning-content.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule, MarkdownComponent]
})
export class MessageTextReasoningContentComponent {

  readonly message = input.required<ChatMessageAppModelView>();
  readonly searchQuery = input<string>('');
  readonly contentTypeFilter = input.required<MessageContentTypeFilter>();

  // Signal to track whether to show markdown or HTML
  protected readonly showMarkdown = signal(true);



  // Combine all reasoning content only, respect contentTypeFilter
  protected readonly content = computed(() => {
    if (!this.contentTypeFilter().reasoning) {
      return null;
    }
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

  // Detect if content is markdown based on common markdown patterns
  protected readonly isMarkdown = computed(() => {
    const text = this.content()?.text ?? '';
    return isMarkdownContent(text);
  });

  // Computed property to determine if markdown should be shown
  protected readonly shouldShowMarkdown = computed(() => {
    return this.isMarkdown() && this.showMarkdown();
  });

  /**
   * Toggles between markdown and HTML display
   */
  protected toggleMarkdownDisplay(): void {
    this.showMarkdown.update(show => !show);
  }

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
