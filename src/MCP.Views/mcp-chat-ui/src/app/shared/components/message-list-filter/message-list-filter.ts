import { ChangeDetectionStrategy, Component, input, output, EventEmitter, signal } from '@angular/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MessageFilters, MessageRoleFilter, MessageContentTypeFilter } from '../../models/message-filters.models';


@Component({
  selector: 'app-message-list-filter',
  templateUrl: './message-list-filter.html',
  styleUrl: './message-list-filter.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatCheckboxModule, MatButtonModule, MatIconModule, MatExpansionModule]
})
export class MessageListFilterComponent {
  readonly disabled = input(false);

  readonly filtersChange = output<MessageFilters>();

  protected readonly filters = signal<MessageFilters>({
    roles: {
      user: true,
      assistant: true,
      tool: false,
      system: false
    },
    contentTypes: {
      text: true,
      reasoning: true,
      error: true,
      functionCall: false,
      functionResult: false
    }
  });

  toggleRole(role: keyof MessageRoleFilter): void {
    this.filters.update(current => {
      const updated = {
        ...current,
        roles: {
          ...current.roles,
          [role]: !current.roles[role]
        }
      };
      this.filtersChange.emit(updated);
      return updated;
    });
  }

  toggleContentType(type: keyof MessageContentTypeFilter): void {
    this.filters.update(current => {
      const updated = {
        ...current,
        contentTypes: {
          ...current.contentTypes,
          [type]: !current.contentTypes[type]
        }
      };
      this.filtersChange.emit(updated);
      return updated;
    });
  }

  resetFilters(): void {
    const reset = {
      roles: {
        user: true,
        assistant: true,
        tool: false,
        system: false
      },
      contentTypes: {
        text: true,
        reasoning: true,
        error: true,
        functionCall: false,
        functionResult: false
      }
    };
    this.filters.set(reset);
    this.filtersChange.emit(reset);
  }
}
