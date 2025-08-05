import { ChangeDetectionStrategy, Component, Input, Output, EventEmitter } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
    selector: 'app-action-icon-button',
    standalone: true,
    imports: [MatIconModule, MatButtonModule, MatTooltipModule],
    templateUrl: './action-icon-button.html',
    styleUrl: './action-icon-button.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActionIconButton {
    @Input() icon: string = '';
    @Input() tooltip: string = '';
    @Input() disabled: boolean = false;
    @Output() action = new EventEmitter<void>();
}
