import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { AiContentAppModelErrorContentAppModelView, ChatMessageAppModelView, AiContentAppModelTextContentAppModelView } from '../../models/chat-completion-view.models';
import { MessageContentTypeFilter } from '../../models/message-filters.models';

@Component({
  selector: 'app-message-error-content',
  templateUrl: './message-error-content.html',
  styleUrl: './message-error-content.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule]
})
export class MessageErrorContentComponent {
  readonly message = input.required<ChatMessageAppModelView>();
  readonly searchQuery = input<string>('');

  readonly contentTypeFilter = input.required<MessageContentTypeFilter>();

  // Combine all text content only
  protected readonly content = computed(() => {
    if (!this.contentTypeFilter().error) {
      return null;
    }

    return (
      this.message().contents.find(
        c => c.$type === 'error'
      ) as AiContentAppModelErrorContentAppModelView || null
    );
  });

}
