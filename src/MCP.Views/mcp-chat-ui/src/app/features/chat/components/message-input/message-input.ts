import { ChangeDetectionStrategy, Component, output, signal, input } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { TextFieldModule } from '@angular/cdk/text-field';

@Component({
  selector: 'app-message-input',
  imports: [MatInputModule, MatButtonModule, MatIconModule, FormsModule, TextFieldModule],
  templateUrl: './message-input.html',
  styleUrl: './message-input.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MessageInput {
  protected readonly messageContent = signal('');
  readonly messageSubmit = output<string>();
  readonly disabled = input<boolean>(false);

  sendMessage(): void {
    const content = this.messageContent().trim();
    if (content) {
      this.messageSubmit.emit(content);
      this.messageContent.set('');
    }
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
}
