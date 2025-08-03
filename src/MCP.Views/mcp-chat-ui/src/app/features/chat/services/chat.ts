import { Injectable, signal, computed, inject } from '@angular/core';
import { ChatResponseAppModelViewBuilder } from '../../../shared/models/chat-completion-view.builder';
import { Observable, map } from 'rxjs';
import { ChatHttpStream } from '../../../core/services/chat-http-stream';
import {
  AiContentAppModelTextContentAppModelView,
  ChatMessageAppModelView,
  ChatRequestView,
  ChatRoleEnumAppModelView,
  ChatResponseAppModelView
} from '../../../shared/models/chat-completion-view.models';
import { ChatMessageAppModelViewBuilder, ChatRequestViewBuilder, AiContentAppModelTextContentAppModelViewBuilder } from '../../../shared/models/chat-completion-view.builder';
import { ChatResponseUpdateAppModel, AiContentAppModelTextContentAppModel, AiContentAppModelTextReasoningContentAppModel, AiContentAppModelErrorContentAppModel } from '../../../shared/models/chat-completion-api.models';
import { fromChatRequestView, updateChatMessageAppModelViewsFromAppModelContents, updateChatResponseAppModelViewFromChatResponseUpdateAppModel } from '../../../shared/models/chat-completion-mapper.models';
import { ChatOptionsService } from './chat-options';
import { exportAsJsonFile } from '../../../shared/utils/file.utils';


@Injectable({
  providedIn: 'root'
})
export class Chat {
  readonly isAwaitingResponse = signal(false);
  private readonly chatStreamService = inject(ChatHttpStream);
  private readonly chatOptionsService = inject(ChatOptionsService);

  readonly messages = signal<ChatMessageAppModelView[]>([]);
  readonly isLoading = signal(false);
  readonly currentOptions = this.chatOptionsService.chatOptionsView;


  // Create placeholder for LLM response using builder
  public readonly chatResponseAppModelView: ChatResponseAppModelView =
    new ChatResponseAppModelViewBuilder().build();
  constructor() {
    this.initializeChatStreamConnection();
  }

  private initializeChatStreamConnection(): void {
    this.chatStreamService.getMessages().subscribe({
      next: (response: ChatResponseUpdateAppModel) => {
        this.handleStreamingResponse(response);
      },
      error: error => {
        this.handleError(error);
      },
      complete: () => {
        console.log('Chat stream completed');
        this.isLoading.set(false);
      }
    });
  }

  private handleError(error: any) {
    console.error('Chat stream error:', error);
    this.isLoading.set(false);

    updateChatMessageAppModelViewsFromAppModelContents(this.chatResponseAppModelView.messages, ChatRoleEnumAppModelView.Assistant, [{
      $type: 'error',
      message: error.message || 'An error occurred while processing the chat response.'
    }], new Date().toISOString());
    this.messages.update(() => [...this.chatResponseAppModelView.messages]);
  }


  sendMessage(content: string): void {
    const trimmedContent = content.trim();
    if (!trimmedContent) return;

    const aAIContentTextContentView = new AiContentAppModelTextContentAppModelViewBuilder()
      .text(trimmedContent)
      .build();

    const userChatMessageView = new ChatMessageAppModelViewBuilder()
      .role(ChatRoleEnumAppModelView.User)
      .contents([aAIContentTextContentView])
      .messageTime(new Date())
      .build();

    this.chatResponseAppModelView.messages.push(userChatMessageView);
    this.isLoading.set(true);
    this.isAwaitingResponse.set(true);
    this.messages.update(() => [...this.chatResponseAppModelView.messages]);

    // Create ChatRequestView using builder
    const chatRequestView = new ChatRequestViewBuilder()
      .messages([...this.chatResponseAppModelView.messages])
      .options(this.currentOptions())
      .build();
    const chatRequest = fromChatRequestView(chatRequestView, this.chatResponseAppModelView.conversationId);
    this.chatStreamService.sendMessage(chatRequest);

  }

  private handleStreamingResponse(response: ChatResponseUpdateAppModel): void {
    updateChatResponseAppModelViewFromChatResponseUpdateAppModel(this.chatResponseAppModelView, response);

    this.messages.update(() => {
      return [...this.chatResponseAppModelView.messages];
    });
    this.isAwaitingResponse.set(false);
    if (response.finish_reason) {
      this.isLoading.set(false);
    }
  }

  clearChat(): void {
    const newResponse = new ChatResponseAppModelViewBuilder().build();
    // Copy properties to preserve reference for signals
    this.assignChatResponseAppModelView(newResponse);
  }

  public assignChatResponseAppModelView(newResponse: ChatResponseAppModelView) {
    Object.assign(this.chatResponseAppModelView, newResponse);
    this.messages.update(() => [...this.chatResponseAppModelView.messages]);
  }

  exportChat(): void {
    const data = this.chatResponseAppModelView;
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const filename = `chat-export-${timestamp}.json`;
    exportAsJsonFile(data, filename);
  }

}

