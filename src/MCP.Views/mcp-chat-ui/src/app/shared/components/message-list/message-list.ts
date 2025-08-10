import { ChangeDetectionStrategy, Component, input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { ChatMessageAppModelView, ChatRoleEnumAppModelView } from '../../models/chat-completion-view.models';
import { MessageFilters, MessageContentTypeFilter, MessageRoleFilter } from '../../models/message-filters.models';
import { SearchConfig, searchFilter } from '../../utils/search.utils';
import { MessageHeaderComponent } from '../message-header/message-header';
import { MessageContentRendererComponent } from '../message-content-renderer/message-content-renderer';
import { MessageFooterComponent } from '../message-footer/message-footer';
import { MessageListFilterComponent } from '../message-list-filter/message-list-filter';

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
    MatCheckboxModule,
    MatExpansionModule,
    MessageHeaderComponent,
    MessageContentRendererComponent,
    MessageFooterComponent,
    MessageListFilterComponent
  ],
  templateUrl: './message-list.html',
  styleUrl: './message-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageList {
  // Filters state managed by child filter component
  protected readonly filters = signal<MessageFilters>({
    roles: {
      user: true,
      assistant: true,
      tool: false,
      system: false
    },
    contentTypes: {
      text: true,
      reasoning: true,
      error: true,
      functionCall: true,
      functionResult: false
    }
  });

  /**
   * Handler for filter changes emitted by child filter component
   */
  onFiltersChange(filters: MessageFilters): void {
    this.filters.set(filters);
  }
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

  // Computed filtered messages based on search query and filters
  protected readonly filteredMessages = computed(() => {
    let messages = this.messages();
    const filters = this.filters();

    // Apply role filtering
    messages = messages.filter(message => {
      switch (message.role) {
        case ChatRoleEnumAppModelView.User:
          return filters.roles.user;
        case ChatRoleEnumAppModelView.Assistant:
          return filters.roles.assistant;
        case ChatRoleEnumAppModelView.Tool:
          return filters.roles.tool;
        case ChatRoleEnumAppModelView.System:
          return filters.roles.system;
        default:
          return false;
      }
    });

    // Apply content type filtering
    messages = messages.filter(message => {
      return message.contents.some(content => {
        switch (content.$type) {
          case 'text':
            return filters.contentTypes.text;
          case 'reasoning':
            return filters.contentTypes.reasoning;
          case 'error':
            return filters.contentTypes.error;
          case 'function_call':
            return filters.contentTypes.functionCall;
          case 'function_result':
            return filters.contentTypes.functionResult;
          default:
            return false;
        }
      });
    });

    // Apply search filtering if query exists
    const query = this.searchQuery().trim();
    if (query) {
      messages = searchFilter(messages, query, this.searchConfig);
    }

    return messages;
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
   * Checks if a message contains error content
   */
  protected isErrorMessage(message: ChatMessageAppModelView): boolean {
    return message?.contents.some(c => c.$type === 'error') || false;
  }



}
