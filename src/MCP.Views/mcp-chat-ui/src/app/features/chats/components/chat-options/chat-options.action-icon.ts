import { ChangeDetectionStrategy, Component, Input, Output, EventEmitter } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
    selector: 'app-action-icon-button',
    standalone: true,
    imports: [MatIconModule, MatButtonModule, MatTooltipModule],
    template: `<button mat-icon-button [matTooltip]="tooltip" [disabled]="disabled" (click)="action.emit()"><mat-icon>{{ icon }}</mat-icon></button>`,
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActionIconButton {
    @Input() icon: string = '';
    @Input() tooltip: string = '';
    @Input() disabled: boolean = false;
    @Output() action = new EventEmitter<void>();
}
