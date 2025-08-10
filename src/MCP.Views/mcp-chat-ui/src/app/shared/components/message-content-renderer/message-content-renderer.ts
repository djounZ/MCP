import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  ChatMessageAppModelView,
  AiContentAppModelView,
  AiContentAppModelErrorContentAppModelView
} from '../../models/chat-completion-view.models';
import { MessageErrorContentComponent } from '../message-error-content/message-error-content';
import { highlightSearchMatches } from '../../utils/search.utils';

@Component({
  selector: 'app-message-content-renderer',
  templateUrl: './message-content-renderer.html',
  styleUrl: './message-content-renderer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatProgressSpinnerModule,
    MessageErrorContentComponent
  ]
})
export class MessageContentRendererComponent {
  readonly message = input.required<ChatMessageAppModelView>();
  readonly searchQuery = input<string>('');
  readonly showSpinner = input<boolean>(false);

  // Combine all text and reasoning content like the old implementation
  protected readonly combinedTextContent = computed(() => {
    return this.message().contents
      .filter(c => c.$type === 'text' || c.$type === 'reasoning')
      .map(c => {
        if (c.$type === 'text' || c.$type === 'reasoning') {
          return c.text || '';
        }
        return '';
      })
      .join('');
  });

  protected readonly displayTextContent = computed(() => {
    const content = this.combinedTextContent();
    const query = this.searchQuery().trim();

    if (!query) {
      return content;
    }

    return highlightSearchMatches(content, query, 'search-highlight');
  });

  // Get non-text content (errors, function calls, etc.) - excluding usage which is handled in footer
  protected readonly nonTextContent = computed(() => {
    return this.message().contents.filter(c =>
      c.$type !== 'text' &&
      c.$type !== 'reasoning' &&
      c.$type !== 'usage'
    );
  });

  protected isContentType(content: AiContentAppModelView, type: string): boolean {
    return content.$type === type;
  }

  protected asErrorContent(content: AiContentAppModelView): AiContentAppModelErrorContentAppModelView {
    return content as AiContentAppModelErrorContentAppModelView;
  }
}
