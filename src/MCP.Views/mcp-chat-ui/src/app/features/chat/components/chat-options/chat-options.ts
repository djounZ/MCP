import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { ChatOptionsService } from '../../services/chat-options';
import { ChatOptionsAppModelView, ChatResponseFormatAppModelView, ChatToolModeAppModelView } from '../../../../shared/models/chat-completion-view.models';

@Component({
  selector: 'app-chat-options',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSliderModule,
    MatCheckboxModule,
    MatChipsModule,
    MatIconModule,
    MatExpansionModule,
    MatTooltipModule,
    MatDividerModule
  ],
  templateUrl: './chat-options.html',
  styleUrl: './chat-options.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatOptionsComponent {
  private readonly chatOptionsService = inject(ChatOptionsService);

  protected readonly options = this.chatOptionsService.options;
  readonly optionsChanged = output<void>();

  protected readonly optionsForm = new FormGroup({
    modelId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    instructions: new FormControl('', { nonNullable: true }),
    temperature: new FormControl(0.7, { nonNullable: true, validators: [Validators.min(0), Validators.max(2)] }),
    maxOutputTokens: new FormControl(2048, { nonNullable: true, validators: [Validators.min(1), Validators.max(8192)] }),
    topP: new FormControl(0.9, { nonNullable: true, validators: [Validators.min(0), Validators.max(1)] }),
    topK: new FormControl(40, { nonNullable: true, validators: [Validators.min(1), Validators.max(100)] }),
    frequencyPenalty: new FormControl(0, { nonNullable: true, validators: [Validators.min(-2), Validators.max(2)] }),
    presencePenalty: new FormControl(0, { nonNullable: true, validators: [Validators.min(-2), Validators.max(2)] }),
    seed: new FormControl<number | null>(null),
    responseFormat: new FormControl<'text' | 'json'>('text', { nonNullable: true }),
    allowMultipleToolCalls: new FormControl(false, { nonNullable: true }),
    toolMode: new FormControl<'none' | 'auto' | 'required'>('none', { nonNullable: true }),
    requiredFunctionName: new FormControl('', { nonNullable: true }),
    jsonSchemaName: new FormControl('', { nonNullable: true }),
    jsonSchemaDescription: new FormControl('', { nonNullable: true })
  });

  protected newStopSequence = new FormControl('', { nonNullable: true });

  protected readonly modelOptions = [
    { value: 'gpt-4', label: 'GPT-4' },
    { value: 'gpt-4-turbo', label: 'GPT-4 Turbo' },
    { value: 'gpt-3.5-turbo', label: 'GPT-3.5 Turbo' },
    { value: 'claude-3', label: 'Claude 3' },
    { value: 'gemini-pro', label: 'Gemini Pro' }
  ];

  protected readonly presets: Array<{ value: 'creative' | 'balanced' | 'precise', label: string, description: string }> = [
    { value: 'creative', label: 'Creative', description: 'High creativity and randomness' },
    { value: 'balanced', label: 'Balanced', description: 'Good balance of creativity and consistency' },
    { value: 'precise', label: 'Precise', description: 'Low randomness, more deterministic' }
  ];

  constructor() {
    // Initialize form with current options
    this.updateFormFromOptions();

    // Subscribe to form changes
    this.optionsForm.valueChanges.subscribe(() => {
      if (this.optionsForm.valid) {
        this.updateOptionsFromForm();
      }
    });
  }

  protected loadPreset(preset: 'creative' | 'balanced' | 'precise'): void {
    this.chatOptionsService.loadPreset(preset);
    this.updateFormFromOptions();
    this.optionsChanged.emit();
  }

  protected resetToDefaults(): void {
    this.chatOptionsService.resetToDefaults();
    this.updateFormFromOptions();
    this.optionsChanged.emit();
  }

  protected addStopSequence(): void {
    const sequence = this.newStopSequence.value.trim();
    if (sequence) {
      this.chatOptionsService.addStopSequence(sequence);
      this.newStopSequence.setValue('');
      this.optionsChanged.emit();
    }
  }

  protected removeStopSequence(index: number): void {
    this.chatOptionsService.removeStopSequence(index);
    this.optionsChanged.emit();
  }

  protected formatSliderValue(value: number, decimals: number = 1): string {
    return value.toFixed(decimals);
  }

  protected getPresetIcon(preset: 'creative' | 'balanced' | 'precise'): string {
    const icons = {
      creative: 'auto_awesome',
      balanced: 'balance',
      precise: 'gps_fixed'
    };
    return icons[preset];
  }

  private updateFormFromOptions(): void {
    const options = this.options();
    this.optionsForm.patchValue({
      modelId: options.modelId || 'gpt-4',
      instructions: options.instructions || '',
      temperature: options.temperature || 0.7,
      maxOutputTokens: options.maxOutputTokens || 2048,
      topP: options.topP || 0.9,
      topK: options.topK || 40,
      frequencyPenalty: options.frequencyPenalty || 0,
      presencePenalty: options.presencePenalty || 0,
      seed: options.seed,
      responseFormat: options.responseFormat?.$type || 'text',
      allowMultipleToolCalls: options.allowMultipleToolCalls || false,
      toolMode: options.toolMode?.$type || 'none',
      requiredFunctionName: (options.toolMode?.$type === 'required' && 'requiredFunctionName' in options.toolMode)
        ? options.toolMode.requiredFunctionName || '' : '',
      jsonSchemaName: (options.responseFormat?.$type === 'json' && 'schemaName' in options.responseFormat)
        ? options.responseFormat.schemaName || '' : '',
      jsonSchemaDescription: (options.responseFormat?.$type === 'json' && 'schemaDescription' in options.responseFormat)
        ? options.responseFormat.schemaDescription || '' : ''
    });
  }

  private updateOptionsFromForm(): void {
    const formValue = this.optionsForm.value;

    const responseFormat: ChatResponseFormatAppModelView = formValue.responseFormat === 'json'
      ? {
          $type: 'json',
          schema: undefined,
          schemaName: formValue.jsonSchemaName || undefined,
          schemaDescription: formValue.jsonSchemaDescription || undefined
        }
      : { $type: 'text' };

    const toolMode: ChatToolModeAppModelView = (() => {
      switch (formValue.toolMode) {
        case 'auto': return { $type: 'auto' };
        case 'required': return {
          $type: 'required',
          requiredFunctionName: formValue.requiredFunctionName || undefined
        };
        default: return { $type: 'none' };
      }
    })();

    const updates: Partial<ChatOptionsAppModelView> = {
      modelId: formValue.modelId!,
      instructions: formValue.instructions || undefined,
      temperature: formValue.temperature!,
      maxOutputTokens: formValue.maxOutputTokens!,
      topP: formValue.topP!,
      topK: formValue.topK!,
      frequencyPenalty: formValue.frequencyPenalty!,
      presencePenalty: formValue.presencePenalty!,
      seed: formValue.seed,
      responseFormat,
      allowMultipleToolCalls: formValue.allowMultipleToolCalls!,
      toolMode
    };

    this.chatOptionsService.updateOptions(updates);
    this.optionsChanged.emit();
  }
}
