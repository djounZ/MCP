import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { Websocket } from '../../../core/services/websocket';
import { ChatMessage, LLMStreamResponse } from '../../../shared/models/message.interface';

@Injectable({
  providedIn: 'root'
})
export class Chat {
  private readonly websocketService = inject(Websocket);

  readonly messages = signal<ChatMessage[]>([]);
  readonly isLoading = signal(false);
  readonly isConnected = computed(() => this.websocketService.isConnected());

  constructor() {
    this.initializeWebSocketConnection();
  }

  private initializeWebSocketConnection(): void {
    this.websocketService.connect().subscribe();

    this.websocketService.getMessages().subscribe((response: LLMStreamResponse) => {
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

    // Send to WebSocket
    this.websocketService.sendMessage(content);

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

  private handleStreamingResponse(response: LLMStreamResponse): void {
    // Only process valid LLMStreamResponse objects
    if (!response || typeof response !== 'object' || typeof response.content !== 'string') {
      // Log invalid or debugging payloads
      console.warn('Received non-LLMStreamResponse payload:', response);
      return;
    }
    this.messages.update(messages => {
      const messageIndex = messages.findIndex(m =>
        !m.isUser && m.isStreaming && m.id === messages[messages.length - 1]?.id
      );

      if (messageIndex !== -1) {
        const updatedMessage = { ...messages[messageIndex] };
        updatedMessage.content += response.content;

        if (response.isComplete) {
          updatedMessage.isStreaming = false;
          this.isLoading.set(false);
        }

        if (response.error) {
          updatedMessage.isError = true;
          updatedMessage.content = response.error;
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
