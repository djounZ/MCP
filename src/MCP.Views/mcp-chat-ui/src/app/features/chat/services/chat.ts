// Type guards for AIContent discriminated union
function isTextContent(c: unknown): c is import('../../../shared/models/message.interface').AIContentTextContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'text';
}
function isReasoningContent(c: unknown): c is import('../../../shared/models/message.interface').AIContentTextReasoningContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'reasoning';
}
function isErrorContent(c: unknown): c is import('../../../shared/models/message.interface').AIContentErrorContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'error';
}
import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ChatHttpStream } from '../../../core/services/chat-http-stream';
import { ChatMessage, ChatResponseUpdate } from '../../../shared/models/message.interface';

@Injectable({
  providedIn: 'root'
})
export class Chat {
  private readonly chatStreamService = inject(ChatHttpStream);

  readonly messages = signal<ChatMessage[]>([]);
  readonly isLoading = signal(false);
  readonly isConnected = computed(() => this.chatStreamService.isConnected());

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
    const userMessage: ChatMessage = {
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
    const llmMessage: ChatMessage = {
      id: this.generateId(),
      content: '',
      isUser: false,
      timestamp: new Date(),
      isStreaming: true
    };

    this.messages.update(messages => [...messages, llmMessage]);
  }

  private handleStreamingResponse(response: ChatResponseUpdate): void {
    // Only process valid ChatResponseUpdate objects
    if (!response || typeof response !== 'object' || !Array.isArray(response.Contents)) {
      // Log invalid or debugging payloads
      console.warn('Received non-ChatResponseUpdate payload:', response);
      return;
    }
    // Extract content and error using type guards
    let content = '';
    let error: string | undefined = undefined;
    let isComplete = false;
    for (const c of response.Contents) {
      if (isTextContent(c)) {
        content += c.Text || '';
      } else if (isReasoningContent(c)) {
        content += c.Text || '';
      } else if (isErrorContent(c)) {
        error = c.Message || 'Unknown error';
      }
    }
    if (response.FinishReason) {
      isComplete = true;
    }
    this.messages.update(messages => {
      const messageIndex = messages.findIndex(m =>
        !m.isUser && m.isStreaming && m.id === messages[messages.length - 1]?.id
      );

      if (messageIndex !== -1) {
        const updatedMessage = { ...messages[messageIndex] };
        updatedMessage.content += content;

        if (isComplete) {
          updatedMessage.isStreaming = false;
          this.isLoading.set(false);
        }

        if (error) {
          updatedMessage.isError = true;
          updatedMessage.content = error;
          updatedMessage.isStreaming = false;
          this.isLoading.set(false);
        }

        const newMessages = [...messages];
        newMessages[messageIndex] = updatedMessage;
        return newMessages;
      }

      return messages;
    });
  }

  clearChat(): void {
    this.messages.set([]);
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}
