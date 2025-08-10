import { ChangeDetectionStrategy, Component, input, computed, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MarkdownComponent } from 'ngx-markdown';
import { AiContentAppModelTextContentAppModelView, ChatMessageAppModelView } from '../../models/chat-completion-view.models';
import { MessageContentTypeFilter } from '../../models/message-filters.models';
import { highlightSearchMatches } from '../../utils/search.utils';
import { isMarkdownContent } from '../../utils/string.utils';

@Component({
  selector: 'app-message-text-content',
  templateUrl: './message-text-content.html',
  styleUrl: './message-text-content.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule, MarkdownComponent]
})
export class MessageTextContentComponent {

  readonly message = input.required<ChatMessageAppModelView>();
  readonly searchQuery = input<string>('');
  readonly contentTypeFilter = input.required<MessageContentTypeFilter>();

  // Signal to track whether to show markdown or HTML
  protected readonly showMarkdown = signal(true);



  // Combine all text content only, respect contentTypeFilter
  protected readonly content = computed(() => {
    if (!this.contentTypeFilter().text) {
      return null;
    }
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
