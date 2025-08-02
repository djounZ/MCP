import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ChatHttpStream } from '../../../core/services/chat-http-stream';
import {
  AiContentAppModelTextContentAppModelView,
  ChatMessageAppModelView,
  ChatRequestView,
  ChatRoleEnumAppModelView,
  ChatResponseUpdateAppModelView
} from '../../../shared/models/chat-completion-view.models';
import { ChatResponseUpdateAppModel, AiContentAppModelTextContentAppModel, AiContentAppModelTextReasoningContentAppModel,  AiContentAppModelErrorContentAppModel } from '../../../shared/models/chat-completion-api.models';
import { fromChatRequestView } from '../../../shared/models/chat-completion-mapper.models';
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

// Utility functions for ChatResponseUpdateAppModelView (UI logic)
export function isUserMessage(message: ChatResponseUpdateAppModelView): boolean {
  return message.role === ChatRoleEnumAppModelView.User;
}

export function isStreaming(message: ChatResponseUpdateAppModelView): boolean {
  return !message.finishReason;
}

export function isErrorMessage(message: ChatResponseUpdateAppModelView): boolean {
  return message.contents.some(c => c.$type === 'error');
}

export function getDisplayContent(message: ChatResponseUpdateAppModelView): string {
  return message.contents
    .filter(c => c.$type === 'text' || c.$type === 'reasoning')
    .map(c => {
      if (c.$type === 'text' || c.$type === 'reasoning') {
        return c.text || '';
      }
      return '';
    })
    .join('');
}

export function getErrorMessage(message: ChatResponseUpdateAppModelView): string {
  const errorContent = message.contents.find(c => c.$type === 'error');
  return errorContent && errorContent.$type === 'error' ? errorContent.message : '';
}

export function getMessageId(message: ChatResponseUpdateAppModelView): string {
  return message.messageId || 'unknown';
}

export function getMessageTime(message: ChatResponseUpdateAppModelView): Date {
  return message.createdAt ? new Date(message.createdAt) : new Date();
}

@Injectable({
  providedIn: 'root'
})
export class Chat {
  private readonly chatStreamService = inject(ChatHttpStream);
  private readonly chatOptionsService = inject(ChatOptionsService);

  readonly messages = signal<ChatResponseUpdateAppModelView[]>([]);
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
    const userMessage: ChatResponseUpdateAppModelView = {
      messageId: this.generateId(),
      role: ChatRoleEnumAppModelView.User,
      contents: [{
        $type: 'text',
        text: content.trim()
      }],
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
    const llmMessage: ChatResponseUpdateAppModelView = {
      messageId: this.generateId(),
      role: ChatRoleEnumAppModelView.Assistant,
      contents: [{
        $type: 'text',
        text: ''
      }],
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

      this.updateChatMessage(messages[idx], response);

      if (response.finish_reason) {
        this.isLoading.set(false);
        // Map API finish reason to view model
        messages[idx].finishReason = response.finish_reason as any;
      }

      return [...messages];
    });
  }

  private updateChatMessage(
    prev: ChatResponseUpdateAppModelView,
    api: ChatResponseUpdateAppModel
  ): void {
    const content = api.contents?.[0];
    if (content && (isTextContent(content) || isReasoningContent(content))) {
      const appendText = content.text || '';
      // Find the text content in the previous message and update it
      const textContent = prev.contents.find(c => c.$type === 'text');
      if (textContent && textContent.$type === 'text') {
        textContent.text = (textContent.text || '') + appendText;
      }
    } else if (content && isErrorContent(content)) {
      // Replace with error content
      prev.contents = [{
        $type: 'error',
        message: content.message || 'An error occurred'
      }];
    }
  }

  clearChat(): void {
    this.messages.set([]);
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }
}
