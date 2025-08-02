// Type guards for AIContent discriminated union
function isTextContent(c: unknown): c is AiContentAppModelTextContentAppModel {
  return !!c && typeof c === 'object' && (c as any).$type === 'text';
}
function isReasoningContent(c: unknown): c is AiContentAppModelTextReasoningContentAppModel {
  return !!c && typeof c === 'object' && (c as any).$type === 'reasoning';
}
function isErrorContent(c: unknown): c is AiContentAppModelErrorContentAppModel {
  return !!c && typeof c === 'object' && (c as any).$type === 'error';
}
import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ChatHttpStream } from '../../../core/services/chat-http-stream';
import { AiContentAppModelTextContentAppModelView, ChatMessageAppModelView, ChatRequestView, ChatRoleEnumAppModelView } from '../../../shared/models/chat-completion-view.models';
import { ChatResponseUpdateAppModel, AiContentAppModelTextContentAppModel, AiContentAppModelTextReasoningContentAppModel,  AiContentAppModelErrorContentAppModel } from '../../../shared/models/chat-completion-api.models';
import { fromChatRequestView } from '../../../shared/models/chat-completion-mapper.models';
import { ChatOptionsService } from './chat-options';

export interface ChatResponseUpdateViewLight {
  id: string;
  content: string;
  isUser: boolean;
  timestamp: Date;
  isStreaming?: boolean;
  isError?: boolean;
}

@Injectable({
  providedIn: 'root'
})


export class Chat {
  private readonly chatStreamService = inject(ChatHttpStream);
  private readonly chatOptionsService = inject(ChatOptionsService);

  readonly messages = signal<ChatResponseUpdateViewLight[]>([]);
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
    const userMessage: ChatResponseUpdateViewLight = {
      id: this.generateId(),
      content: content.trim(),
      isUser: true,
      timestamp: new Date()
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
      options: this.currentOptions()
    };
    const chatRequest = fromChatRequestView(chatRequestView);
    this.chatStreamService.sendMessage(chatRequest);

    // Create placeholder for LLM response
    const llmMessage = {
      id: this.generateId(),
      content: '',
      isUser: false,
      timestamp: new Date(),
      isStreaming: true
    };

    this.messages.update(messages => [...messages, llmMessage]);

  }

  private handleStreamingResponse(response: ChatResponseUpdateAppModel): void {
    // Only process valid ChatResponseUpdate objects
    if (!response || typeof response !== 'object' || !Array.isArray(response.contents)) {
      // Log invalid or debugging payloads
      console.warn('Received non-ChatResponseUpdate payload:', response);
      return;
    }
    this.messages.update(messages => {
      const idx = messages.length - 1;
      if (idx < 0) return messages;
      this.updateChatMessageViewLight(messages[idx], response, { isStreaming: !response.finish_reason });
      if (response.finish_reason) {
        this.isLoading.set(false);
        messages[idx].isStreaming = false;
      }
      return [...messages];
    });
  }

  updateChatMessageViewLight(
  prev: ChatResponseUpdateViewLight,
  api: ChatResponseUpdateAppModel,
  opts?: { isStreaming?: boolean }
): void {
  let appendText = '';
  const content = api.contents?.[0];
  if (content && (isTextContent(content) || isReasoningContent(content))) {
    appendText = content.text || '';
  } else if (content && isErrorContent(content)) {
    appendText = content.message || '';
    prev.isError = true;
  }
  prev.content += appendText;
  prev.isStreaming = opts?.isStreaming;
}

  clearChat(): void {
    this.messages.set([]);
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}
