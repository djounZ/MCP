import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-dev-in-progress',
  standalone: true,
  imports: [MatCardModule],
  templateUrl: './dev-in-progress.html',
  styleUrl: './dev-in-progress.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DevInProgressComponent { }
