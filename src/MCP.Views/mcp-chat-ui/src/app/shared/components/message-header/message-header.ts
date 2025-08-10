import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-message-header',
  templateUrl: './message-header.html',
  styleUrl: './message-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, MatTooltipModule]
})
export class MessageHeaderComponent {
  readonly role = input.required<string>();
  readonly messageTime = input.required<Date>();

  protected readonly authorName = computed(() => 
    this.role() === 'user' ? 'You' : 'AI Assistant'
  );

  protected readonly formattedTime = computed(() => 
    this.getFormattedTime(this.messageTime())
  );

  protected readonly fullDateTooltip = computed(() => 
    this.messageTime()
  );

  private getFormattedTime(time: Date): string {
    const now = new Date();
    const diffInMinutes = Math.round((now.getTime() - time.getTime()) / (1000 * 60));
    
    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
    
    const diffInHours = Math.round(diffInMinutes / 60);
    if (diffInHours < 24) return `${diffInHours}h ago`;
    
    const diffInDays = Math.round(diffInHours / 24);
    return `${diffInDays}d ago`;
  }
}
