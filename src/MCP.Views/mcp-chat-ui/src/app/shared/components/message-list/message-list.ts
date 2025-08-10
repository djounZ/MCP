import { ChangeDetectionStrategy, Component, input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { ChatMessageAppModelView } from '../../models/chat-completion-view.models';
import { SearchConfig, searchFilter, highlightSearchMatches } from '../../utils/search.utils';
import { MessageHeaderComponent } from '../message-header/message-header';
import { MessageContentComponent } from '../message-content/message-content';
import { MessageFooterComponent } from '../message-footer/message-footer';

@Component({
  selector: 'app-message-list',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTooltipModule,
    MatButtonModule,
    MessageHeaderComponent,
    MessageContentComponent,
    MessageFooterComponent
  ],
  templateUrl: './message-list.html',
  styleUrl: './message-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageList {
  readonly messages = input.required<ChatMessageAppModelView[]>();
  readonly isLoading = input.required<boolean>();
  readonly isAwaitingResponse = input.required<boolean>();

  // Search functionality
  protected readonly searchQuery = signal('');

  // Search configuration for messages
  private readonly searchConfig: SearchConfig<ChatMessageAppModelView> = {
    extractText: (message) => {
      const textContent = message.contents
        .filter(c => c.$type === 'text' || c.$type === 'reasoning')
        .map(c => c.text || '')
        .filter(text => text.length > 0);

      // Also include role for searching
      const roleText = message.role === 'user' ? 'You' : 'AI Assistant';

      return [roleText, ...textContent];
    },
    caseSensitive: false,
    wholeWordOnly: false
  };

  // Computed filtered messages based on search query
  protected readonly filteredMessages = computed(() => {
    const query = this.searchQuery().trim();
    if (!query) {
      return this.messages();
    }
    return searchFilter(this.messages(), query, this.searchConfig);
  });

  // Computed search result info
  protected readonly searchResultInfo = computed(() => {
    const query = this.searchQuery().trim();
    if (!query) {
      return null;
    }

    const filteredCount = this.filteredMessages().length;
    const totalCount = this.messages().length;

    return {
      filtered: filteredCount,
      total: totalCount,
      isActive: true
    };
  });

  /**
   * Updates the search query
   */
  protected updateSearchQuery(query: string): void {
    this.searchQuery.set(query);
  }

  /**
   * Clears the search query
   */
  protected clearSearch(): void {
    this.searchQuery.set('');
  }

  /**
   * Gets display content with search highlighting if active
   */
  protected getDisplayContentWithHighlight(message: ChatMessageAppModelView): string {
    const content = this.getDisplayContent(message);
    const query = this.searchQuery().trim();

    if (!query) {
      return content;
    }

    return highlightSearchMatches(content, query, 'search-highlight');
  }

  isErrorMessage(message: ChatMessageAppModelView): boolean {
    return message?.contents.some(c => c.$type === 'error') || false;
  }

  getDisplayContent(message: ChatMessageAppModelView): string {

    return message.contents
      .filter(c => c.$type === 'text' || c.$type === 'reasoning')
      .map(c => {
        if (c.$type === 'text' || c.$type === 'reasoning') {
          return c.text || '';
        }
        return '';
      })
      .join('');
  }

  getErrorMessage(message: ChatMessageAppModelView): string {
    const errorContent = message.contents.find(c => c.$type === 'error');
    return errorContent && errorContent.$type === 'error' ? errorContent.message : '';
  }
}
