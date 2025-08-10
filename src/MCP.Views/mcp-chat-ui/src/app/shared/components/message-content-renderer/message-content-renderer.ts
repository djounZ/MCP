import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  ChatMessageAppModelView,
  AiContentAppModelView,
  AiContentAppModelErrorContentAppModelView,
  AiContentAppModelTextContentAppModelView
} from '../../models/chat-completion-view.models';
import { MessageErrorContentComponent } from '../message-error-content/message-error-content';
import { MessageTextContentComponent } from '../message-text-content/message-text-content';

@Component({
  selector: 'app-message-content-renderer',
  templateUrl: './message-content-renderer.html',
  styleUrl: './message-content-renderer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatProgressSpinnerModule,
    MessageErrorContentComponent,
    MessageTextContentComponent
  ]
})
export class MessageContentRendererComponent {
  readonly message = input.required<ChatMessageAppModelView>();
  readonly searchQuery = input<string>('');
  readonly showSpinner = input<boolean>(false);

  // Combine all text and reasoning content into a single text content object
  protected readonly combinedTextContent = computed(() => {
    const combinedText = this.message().contents
      .filter(c => c.$type === 'text' || c.$type === 'reasoning')
      .map(c => {
        if (c.$type === 'text' || c.$type === 'reasoning') {
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
