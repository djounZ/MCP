import { Injectable, signal, computed } from '@angular/core';
import { ChatOptionsAppModelView } from '../../../shared/models/chat-completion-view.models';

const DEFAULT_OPTIONS: ChatOptionsAppModelView = {
  temperature: 0.7,
  maxOutputTokens: 2048,
  topP: 0.9,
  topK: 40,
  frequencyPenalty: 0,
  presencePenalty: 0,
  responseFormat: { $type: 'text' },
  modelId: 'gpt-4',
  stopSequences: [],
  allowMultipleToolCalls: false,
  toolMode: { $type: 'none' }
};

@Injectable({
  providedIn: 'root'
})
export class ChatOptionsService {
  private readonly optionsState = signal<ChatOptionsAppModelView>(DEFAULT_OPTIONS);

  readonly options = this.optionsState.asReadonly();
  readonly chatOptionsView = this.optionsState.asReadonly();

  updateOptions(updates: Partial<ChatOptionsAppModelView>): void {
    this.optionsState.update(current => ({ ...current, ...updates }));
  }

  resetToDefaults(): void {
    this.optionsState.set(DEFAULT_OPTIONS);
  }

  addStopSequence(sequence: string): void {
    if (!sequence.trim()) return;
    this.optionsState.update(state => ({
      ...state,
      stopSequences: [...(state.stopSequences ?? []), sequence.trim()]
    }));
  }

  removeStopSequence(index: number): void {
    this.optionsState.update(state => ({
      ...state,
      stopSequences: (state.stopSequences ?? []).filter((_, i) => i !== index)
    }));
  }

  loadPreset(preset: 'creative' | 'balanced' | 'precise'): void {
    const presets: Record<string, Partial<ChatOptionsAppModelView>> = {
      creative: {
        temperature: 1.0,
        topP: 0.95,
        topK: 50,
        frequencyPenalty: 0.2,
        presencePenalty: 0.1
      },
      balanced: {
        temperature: 0.7,
        topP: 0.9,
        topK: 40,
        frequencyPenalty: 0,
        presencePenalty: 0
      },
      precise: {
        temperature: 0.1,
        topP: 0.1,
        topK: 1,
        frequencyPenalty: 0,
        presencePenalty: 0
      }
    };
    this.updateOptions(presets[preset]);
  }
}
