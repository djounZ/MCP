import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  ChatMessageAppModelView,
  AiContentAppModelView,
  AiContentAppModelErrorContentAppModelView,
  AiContentAppModelTextContentAppModelView,
  AiContentAppModelTextReasoningContentAppModelView
} from '../../models/chat-completion-view.models';
import { MessageErrorContentComponent } from '../message-error-content/message-error-content';
import { MessageTextContentComponent } from '../message-text-content/message-text-content';
import { MessageTextReasoningContentComponent } from '../message-text-reasoning-content/message-text-reasoning-content';
import { MessageFunctionCallContentComponent } from '../message-function-call-content/message-function-call-content';
import { MessageFunctionResultContentComponent } from '../message-function-result-content/message-function-result-content';

@Component({
  selector: 'app-message-content-renderer',
  templateUrl: './message-content-renderer.html',
  styleUrl: './message-content-renderer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatProgressSpinnerModule,
    MessageErrorContentComponent,
    MessageTextContentComponent,
    MessageTextReasoningContentComponent,
    MessageFunctionCallContentComponent,
    MessageFunctionResultContentComponent
  ]
})
export class MessageContentRendererComponent {
  readonly message = input.required<ChatMessageAppModelView>();
  readonly searchQuery = input<string>('');
  readonly showSpinner = input<boolean>(false);

  // Combine all text content only
  protected readonly combinedTextContent = computed(() => {
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

  // Combine all reasoning content only
  protected readonly combinedTextReasoningContent = computed(() => {
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

  // Get non-text content (errors, function calls, etc.) - excluding usage which is handled in footer
  protected readonly nonTextContent = computed(() => {
    return this.message().contents.filter(c =>
      c.$type !== 'text' &&
      c.$type !== 'reasoning' &&
      c.$type !== 'usage' &&
      c.$type !== 'error' &&
      c.$type !== 'function_call' &&
      c.$type !== 'function_result'
    );
  });

  protected isContentType(content: AiContentAppModelView, type: string): boolean {
    return content.$type === type;
  }

  protected asErrorContent(content: AiContentAppModelView): AiContentAppModelErrorContentAppModelView {
    return content as AiContentAppModelErrorContentAppModelView;
  }
}
