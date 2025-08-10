import { ChangeDetectionStrategy, Component, input, output, EventEmitter } from '@angular/core';
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
    readonly filters = input.required<MessageFilters>();
    readonly disabled = input(false);

    readonly toggleRole = output<keyof MessageRoleFilter>();
    readonly toggleContentType = output<keyof MessageContentTypeFilter>();
    readonly resetFilters = output<void>();
}
