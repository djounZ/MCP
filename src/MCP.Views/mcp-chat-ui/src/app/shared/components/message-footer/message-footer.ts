import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ChatMessageAppModelView } from '../../models/chat-completion-view.models';
import { MessageUsageContentComponent } from '../message-usage-content/message-usage-content';

@Component({
  selector: 'app-message-footer',
  templateUrl: './message-footer.html',
  styleUrl: './message-footer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MessageUsageContentComponent]
})
export class MessageFooterComponent {
  readonly message = input.required<ChatMessageAppModelView>();
}
