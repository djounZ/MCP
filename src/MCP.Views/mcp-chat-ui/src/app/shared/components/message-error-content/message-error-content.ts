import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { AiContentAppModelErrorContentAppModelView } from '../../models/chat-completion-view.models';

@Component({
    selector: 'app-message-error-content',
    templateUrl: './message-error-content.html',
    styleUrl: './message-error-content.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatIconModule]
})
export class MessageErrorContentComponent {
    readonly content = input.required<AiContentAppModelErrorContentAppModelView>();
    readonly searchQuery = input<string>('');
}
