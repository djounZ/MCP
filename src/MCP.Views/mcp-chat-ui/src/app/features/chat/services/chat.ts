import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ChatHttpStream } from '../../../core/services/chat-http-stream';
import {
  AiContentAppModelTextContentAppModelView,
  ChatMessageAppModelView,
  ChatRequestView,
  ChatRoleEnumAppModelView,
  ChatResponseAppModelView
} from '../../../shared/models/chat-completion-view.models';
import { ChatResponseUpdateAppModel, AiContentAppModelTextContentAppModel, AiContentAppModelTextReasoningContentAppModel, AiContentAppModelErrorContentAppModel } from '../../../shared/models/chat-completion-api.models';
import { fromChatRequestView, updateChatMessageAppModelViewsFromAppModelContents, updateChatResponseAppModelViewFromChatResponseUpdateAppModel } from '../../../shared/models/chat-completion-mapper.models';
import { ChatOptionsService } from './chat-options';

// Type guards for AIContent discriminated union (API model)
function isTextContent(c: unknown): c is AiContentAppModelTextContentAppModel {
  return !!c && typeof c === 'object' && (c as any).$type === 'text';
}
function isReasoningContent(c: unknown): c is AiContentAppModelTextReasoningContentAppModel {
  return !!c && typeof c === 'object' && (c as any).$type === 'reasoning';
}
function isErrorContent(c: unknown): c is AiContentAppModelErrorContentAppModel {
  return !!c && typeof c === 'object' && (c as any).$type === 'error';
}


@Injectable({
  providedIn: 'root'
})
export class Chat {
  private readonly chatStreamService = inject(ChatHttpStream);
  private readonly chatOptionsService = inject(ChatOptionsService);

  readonly messages = signal<ChatMessageAppModelView[]>([]);
  readonly isLoading = signal(false);
  readonly currentOptions = this.chatOptionsService.chatOptionsView;


  // Create placeholder for LLM response
  public readonly chatResponseAppModelView: ChatResponseAppModelView = {
    messages: [],
    responseId: null,
    conversationId: null,
    modelId: null,
    createdAt: new Date().toISOString(),
    finishReason: null // null means still streaming
  };
  constructor() {
    this.initializeChatStreamConnection();
  }

  private initializeChatStreamConnection(): void {
    this.chatStreamService.getMessages().subscribe({
      next: (response: ChatResponseUpdateAppModel) => {
        this.handleStreamingResponse(response);
      },
      error: error => {
        console.error('Chat stream error:', error);
        this.isLoading.set(false);

        updateChatMessageAppModelViewsFromAppModelContents(this.chatResponseAppModelView.messages, ChatRoleEnumAppModelView.Assistant, [{
          $type: 'error',
          message: error.message || 'An error occurred while processing the chat response.'
        }], new Date().toISOString());
        this.messages.update(() => [...this.chatResponseAppModelView.messages]);
      },
      complete: () => {
        console.log('Chat stream completed');
        this.isLoading.set(false);
      }
    });
  }

  sendMessage(content: string): void {
    if (!content.trim()) return;



    const aAIContentTextContentView: AiContentAppModelTextContentAppModelView = {
      $type: 'text',
      text: content.trim()
    };
    const userChatMessageView: ChatMessageAppModelView = {
      contents: [aAIContentTextContentView],
      role: ChatRoleEnumAppModelView.User,
      messageTime: new Date()
    };

    this.chatResponseAppModelView.messages.push(userChatMessageView);
    this.isLoading.set(true);
    this.messages.update(() => [...this.chatResponseAppModelView.messages]);

    // Create ChatRequestView
    const chatRequestView: ChatRequestView = {
      messages: [...this.chatResponseAppModelView.messages],
      options: this.currentOptions()
    };
    const chatRequest = fromChatRequestView(chatRequestView);
    this.chatStreamService.sendMessage(chatRequest);

  }

  private handleStreamingResponse(response: ChatResponseUpdateAppModel): void {
    updateChatResponseAppModelViewFromChatResponseUpdateAppModel(this.chatResponseAppModelView, response);

    this.messages.update(() => {
      return [...this.chatResponseAppModelView.messages];
    });
    if (response.finish_reason) {
      this.isLoading.set(false);
    }
  }

  clearChat(): void {
    this.chatResponseAppModelView.messages = [];
    this.chatResponseAppModelView.responseId = null;
    this.chatResponseAppModelView.conversationId = null;
    this.chatResponseAppModelView.modelId = null;
    this.chatResponseAppModelView.createdAt = new Date().toISOString();
    this.chatResponseAppModelView.finishReason = null; // Reset finish reason
    this.messages.update(() => [...this.chatResponseAppModelView.messages]);
  }

  private generateId(): string {
    // RFC4122 version 4 compliant
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }
}
