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
import { fromChatRequestView, fromChatResponseUpdateAppModelToChatResponseAppModelView, updateViewFromChatResponseUpdateAppModelTo } from '../../../shared/models/chat-completion-mapper.models';
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

  readonly messages = signal<ChatResponseAppModelView[]>([]);
  readonly isLoading = signal(false);
  readonly currentOptions = this.chatOptionsService.chatOptionsView;

  constructor() {
    this.initializeChatStreamConnection();
  }

  private initializeChatStreamConnection(): void {
    this.chatStreamService.getMessages().subscribe((response: ChatResponseUpdateAppModel) => {
      this.handleStreamingResponse(response);
    });
  }

  sendMessage(content: string): void {
    if (!content.trim()) return;

    // Add user message
    const userMessage: ChatResponseAppModelView = {
      messages: [{
        role: ChatRoleEnumAppModelView.User,
        contents: [{
          $type: 'text',
          text: content.trim()
        }]
      }],
      responseId: null,
      conversationId: null,
      modelId: null,
      createdAt: new Date().toISOString(),
      finishReason: null
    };

    this.messages.update(messages => [...messages, userMessage]);
    this.isLoading.set(true);

    const aAIContentTextContentView: AiContentAppModelTextContentAppModelView = {
      $type: 'text',
      text: content.trim()
    };
    const userChatMessageView: ChatMessageAppModelView = {
      contents: [aAIContentTextContentView],
      role: ChatRoleEnumAppModelView.User
    };

    // Create ChatRequestView
    const chatRequestView: ChatRequestView = {
      messages: [userChatMessageView],
      //options: this.currentOptions()
    };
    const chatRequest = fromChatRequestView(chatRequestView);
    this.chatStreamService.sendMessage(chatRequest);

    // Create placeholder for LLM response
    const llmMessage: ChatResponseAppModelView = {
      messages: [{
        role: ChatRoleEnumAppModelView.Assistant,
        contents: []
      }],
      responseId: null,
      conversationId: null,
      modelId: null,
      createdAt: new Date().toISOString(),
      finishReason: null // null means still streaming
    };

    this.messages.update(messages => [...messages, llmMessage]);
  }

  private handleStreamingResponse(response: ChatResponseUpdateAppModel): void {
    // Only process valid ChatResponseUpdate objects
    if (!response || typeof response !== 'object' || !Array.isArray(response.contents)) {
      console.warn('Received non-ChatResponseUpdate payload:', response);
      return;
    }

    this.messages.update(messages => {
      const idx = messages.length - 1;
      if (idx < 0) return messages;

      // Use our mapper function to update the existing message
      updateViewFromChatResponseUpdateAppModelTo(messages[idx], response);

      if (response.finish_reason) {
        this.isLoading.set(false);
      }

      return [...messages];
    });
  }

  clearChat(): void {
    this.messages.set([]);
  }

  private generateId(): string {
    // RFC4122 version 4 compliant
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }
}
