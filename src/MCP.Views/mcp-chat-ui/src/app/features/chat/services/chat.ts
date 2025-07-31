// Type guards for AIContent discriminated union
function isTextContent(c: unknown): c is import('../../../shared/models/chat-api.model').AIContentTextContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'text';
}
function isReasoningContent(c: unknown): c is import('../../../shared/models/chat-api.model').AIContentTextReasoningContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'reasoning';
}
function isErrorContent(c: unknown): c is import('../../../shared/models/chat-api.model').AIContentErrorContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'error';
}
import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ChatHttpStream } from '../../../core/services/chat-http-stream';
import { ChatResponseUpdateView } from '../../../shared/models/chat-view-message.model';
import { ChatResponseUpdate } from '../../../shared/models/chat-api.model';
import { updateChatMessageView } from '../../../shared/models/chat-view-message.mapper';

@Injectable({
  providedIn: 'root'
})
export class Chat {
  private readonly chatStreamService = inject(ChatHttpStream);

  readonly messages = signal<ChatResponseUpdateView[]>([]);
  readonly isLoading = signal(false);

  constructor() {
    this.initializeChatStreamConnection();
  }

  private initializeChatStreamConnection(): void {
    this.chatStreamService.connect().subscribe();

    this.chatStreamService.getMessages().subscribe((response: ChatResponseUpdate) => {
      this.handleStreamingResponse(response);
    });
  }

  sendMessage(content: string): void {
    if (!content.trim()) return;

    // Add user message
    const userMessage: ChatResponseUpdateView = {
      id: this.generateId(),
      content: content.trim(),
      isUser: true,
      timestamp: new Date()
    };

    this.messages.update(messages => [...messages, userMessage]);
    this.isLoading.set(true);

    // Send to HTTP stream
    this.chatStreamService.sendMessage(content);

    // Create placeholder for LLM response
    const llmMessage = {
      id: this.generateId(),
      content: '',
      isUser: false,
      timestamp: new Date(),
      isStreaming: true
    };

    this.messages.update(messages => [...messages, llmMessage ]);

  }

  private handleStreamingResponse(response: ChatResponseUpdate): void {
    // Only process valid ChatResponseUpdate objects
    if (!response || typeof response !== 'object' || !Array.isArray(response.Contents)) {
      // Log invalid or debugging payloads
      console.warn('Received non-ChatResponseUpdate payload:', response);
      return;
    }
    this.messages.update(messages => {
      const idx = messages.length - 1;
      if (idx < 0) return messages;
      updateChatMessageView(messages[idx], response, { isStreaming: !response.FinishReason });
      if (response.FinishReason) {
        this.isLoading.set(false);
        messages[idx].isStreaming = false;
      }
      return [...messages];
    });
  }

  clearChat(): void {
    this.messages.set([]);
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}
