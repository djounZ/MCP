import { ChangeDetectionStrategy, Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { McpServerConfigurationItem } from '../../../../shared/models/mcp-server-config-api.models';

export interface ServerFormData {
  serverName?: string;
  config?: McpServerConfigurationItem;
  isEdit?: boolean;
}

@Component({
  selector: 'app-server-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatCardModule
  ],
  templateUrl: './server-form.html',
  styleUrl: './server-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ServerFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ServerFormComponent>);
  private readonly data = inject<ServerFormData>(MAT_DIALOG_DATA);

  protected readonly serverForm: FormGroup;
  protected readonly isEdit = signal(this.data?.isEdit ?? false);
  protected readonly argsArray = signal<string[]>([]);
  protected readonly envArray = signal<Array<{ key: string, value: string }>>([]);

  protected readonly formTitle = computed(() =>
    this.isEdit() ? 'Edit MCP Server' : 'Add New MCP Server'
  );

  constructor() {
    this.serverForm = this.fb.group({
      serverName: [this.data?.serverName ?? '', [Validators.required, Validators.minLength(1)]],
      type: [this.data?.config?.type ?? 'stdio', [Validators.required]],
      category: [this.data?.config?.category ?? ''],
      command: [this.data?.config?.command ?? ''],
      url: [this.data?.config?.url ?? ''],
      newArg: [''],
      newEnvKey: [''],
      newEnvValue: ['']
    });

    // Initialize arrays from existing data
    if (this.data?.config?.args) {
      this.argsArray.set([...this.data.config.args]);
    }

    if (this.data?.config?.env) {
      const envEntries = Object.entries(this.data.config.env).map(([key, value]) => ({ key, value }));
      this.envArray.set(envEntries);
    }

    // Set up conditional validators
    this.setupConditionalValidators();
  }

  private setupConditionalValidators(): void {
    this.serverForm.get('type')?.valueChanges.subscribe(type => {
      const commandControl = this.serverForm.get('command');
      const urlControl = this.serverForm.get('url');

      if (type === 'stdio') {
        commandControl?.setValidators([Validators.required]);
        urlControl?.clearValidators();
      } else if (type === 'http') {
        urlControl?.setValidators([Validators.required, Validators.pattern('https?://.+')]);
        commandControl?.clearValidators();
      }

      commandControl?.updateValueAndValidity();
      urlControl?.updateValueAndValidity();
    });
  }

  addArgument(): void {
    const newArg = this.serverForm.get('newArg')?.value?.trim();
    if (newArg) {
      this.argsArray.update(args => [...args, newArg]);
      this.serverForm.patchValue({ newArg: '' });
    }
  }

  removeArgument(index: number): void {
    this.argsArray.update(args => args.filter((_, i) => i !== index));
  }

  addEnvironmentVariable(): void {
    const key = this.serverForm.get('newEnvKey')?.value?.trim();
    const value = this.serverForm.get('newEnvValue')?.value?.trim();

    if (key && value) {
      this.envArray.update(env => [...env, { key, value }]);
      this.serverForm.patchValue({ newEnvKey: '', newEnvValue: '' });
    }
  }

  removeEnvironmentVariable(index: number): void {
    this.envArray.update(env => env.filter((_, i) => i !== index));
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.serverForm.valid) {
      const formValue = this.serverForm.value;

      const config: McpServerConfigurationItem = {
        type: formValue.type,
        category: formValue.category || null,
        command: formValue.type === 'stdio' ? formValue.command : null,
        url: formValue.type === 'http' ? formValue.url : null,
        args: this.argsArray().length > 0 ? this.argsArray() : null,
        env: this.envArray().length > 0 ? this.envArray().reduce((acc, item) => {
          acc[item.key] = item.value;
          return acc;
        }, {} as { [key: string]: string }) : null
      };

      const result = {
        serverName: formValue.serverName,
        config: config,
        isEdit: this.isEdit()
      };

      this.dialogRef.close(result);
    }
  }

  protected readonly serverTypes = [
    { value: 'stdio', label: 'Standard I/O', icon: 'terminal' },
    { value: 'http', label: 'HTTP', icon: 'http' }
  ];
}
