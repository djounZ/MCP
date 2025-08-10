import { ChangeDetectionStrategy, Component, input, computed } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  ChatMessageAppModelView,
  AiContentAppModelView,
  AiContentAppModelErrorContentAppModelView,
  AiContentAppModelTextContentAppModelView,
  AiContentAppModelTextReasoningContentAppModelView
} from '../../models/chat-completion-view.models';
import { MessageContentTypeFilter } from '../../models/message-filters.models';
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
  readonly contentTypeFilter = input.required<MessageContentTypeFilter>();
  // Only pass props to child components, let them handle filtering
  // Optionally, keep nonTextContent for unknown types
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
}
