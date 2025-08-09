import { ChangeDetectionStrategy, Component, inject, signal, ViewChild, ElementRef, effect } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MessageList } from '../../../shared/components/message-list/message-list';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmationDialog, ConfirmationDialogData } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { ChatOptionsComponent } from '../components/chat-options/chat-options';
import { ActionIconButton } from '../../../shared/components/action-icon-button/action-icon-button';
import { Chat as ChatService } from '../services/chat';
import { ChatHttpStream } from '../../../core/services/chat-http-stream';
import { MessageInput } from '../../../shared/components/message-input/message-input';
import { formatFromSnakeCase } from '../../../shared/utils/string.utils';
import { AiProviderAppModel } from '../../../shared/models/chat-completion-api.models';
import { AiProviderAppModelView, AiProviderAiModelAppModelView } from '../../../shared/models/chat-completion-view.models';
import { toAiProviderAppModelViewArray } from '../../../shared/models/chat-completion-mapper.models';

@Component({
  selector: 'app-basic-chat',
  imports: [
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatTooltipModule,
    MatSidenavModule,
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatDialogModule,
    MessageList,
    MessageInput,
    ChatOptionsComponent,
    ActionIconButton
    , ReactiveFormsModule
  ],
  templateUrl: './basic-chat.html',
  styleUrl: './basic-chat.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BasicChatComponent {
  formatProviderName(provider: string): string {
    return formatFromSnakeCase(provider);
  }
  protected readonly providers = signal<AiProviderAppModelView[]>([]);
  protected readonly selectedProvider = signal<AiProviderAppModelView | null>(null);
  protected readonly chatHttpStream = inject(ChatHttpStream);

  // Models for selected provider
  protected readonly models = signal<AiProviderAiModelAppModelView[]>([]);
  protected readonly selectedModel = signal<AiProviderAiModelAppModelView | null>(null);

  constructor(private dialog: MatDialog) {
    this.loadProviders();
    // React to provider changes
    effect(() => {
      const provider = this.selectedProvider();
      const models = provider?.models ?? [];
      this.models.set(models);
      // If current selectedModel is not in models, reset
      if (!models.length) {
        this.selectedModel.set(null);
      } else if (!models.some(m => m.id === this.selectedModel()?.id)) {
        this.selectedModel.set(models[0]);
      }
    });
  }

  private async loadProviders() {
    const result = await this.chatHttpStream.getProviders();
    const viewProviders = toAiProviderAppModelViewArray(result);
    this.providers.set(viewProviders);
    if (!this.selectedProvider() && viewProviders.length > 0) {
      this.selectedProvider.set(viewProviders[0]);
    }
  }

  confirmClearChat(): void {
    const dialogRef = this.dialog.open<ConfirmationDialog, ConfirmationDialogData, boolean>(ConfirmationDialog, {
      data: {
        title: 'Clear Chat',
        message: 'Are you sure you want to clear the chat?'
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.clearChat();
      }
    });
  }
  @ViewChild(MessageInput) messageInputComponent?: MessageInput;

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  private readonly _focusEffect = effect(() => {
    if (!this.isLoading() && this.messageInputComponent) {
      setTimeout(() => {
        this.messageInputComponent?.focusInput();
      });
    }
  });
  protected readonly chatService = inject(ChatService);

  protected readonly messages = this.chatService.messages;
  protected readonly isLoading = this.chatService.isLoading;
  protected readonly showOptions = signal(false);

  onMessageSubmit(content: string): void {
    this.chatService.sendMessage(content, this.selectedProvider(), this.selectedModel());
  }

  onToolsConfigured(tools: Map<string, import("../../../shared/models/chat-completion-view.models").AiToolAppModelView[]> | null): void {
    this.chatService.updateSelectedTools(tools);
  }

  clearChat(): void {
    this.chatService.clearChat();
  }

  exportChat(): void {
    this.chatService.exportChat();
  }

  triggerImport(): void {
    this.fileInput.nativeElement.click();
  }

  async importChat(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const { readJsonFile } = await import('../../../shared/utils/file.utils');
    try {
      const imported = await readJsonFile(file);
      // Validate imported structure if needed
      if (imported && typeof imported === 'object' && 'messages' in imported) {

        const chatResponseAppModelView = imported as import("../../../shared/models/chat-completion-view.models").ChatResponseAppModelView;
        this.chatService.assignChatResponseAppModelView(chatResponseAppModelView);
      }
    } catch (err) {
      // Optionally show error dialog/toast
      console.error('Failed to import chat:', err);
    }
    input.value = '';
  }

  toggleOptions(): void {
    this.showOptions.update(show => !show);
  }

  onOptionsChanged(): void {
    // Options have been updated, could show a toast notification
    console.log('Chat options updated');
  }
}

