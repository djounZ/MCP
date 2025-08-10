import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
    selector: 'app-message-content',
    templateUrl: './message-content.html',
    styleUrl: './message-content.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [MatIconModule, MatProgressSpinnerModule]
})
export class MessageContentComponent {
    readonly isError = input.required<boolean>();
    readonly errorMessage = input<string>('');
    readonly displayContent = input.required<string>();
    readonly showSpinner = input<boolean>(false);
}
