import { Injectable, signal } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ChatFinishReasonAppModel, ChatRequest, ChatResponseUpdateAppModel, ChatRoleEnumAppModel, AiProviderAppModel } from '../../shared/models/chat-completion-api.models';

@Injectable({
  providedIn: 'root'
})
export class ChatHttpStream {

  private readonly messageSubject$ = new Subject<ChatResponseUpdateAppModel>();

  private readonly apiBaseUrl = 'http://localhost:5200';
  private readonly endpoints = {
    completionsStream: '/api/chat/completions/stream',
    providers: '/api/chat/providers'
  };


  async getProviders(): Promise<AiProviderAppModel[]> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.providers, {
        method: 'GET'
      });
      if (!response.ok) {
        throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      }
      return response.json() as Promise<AiProviderAppModel[]>;
    } catch (error) {
      console.error('Failed to fetch providers:', error);
      return [];
    }
  }

  async sendMessage(message: ChatRequest): Promise<void> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.completionsStream, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(message)
      });
      if (!response.ok) {
        throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      }
      const reader = response.body!.getReader();
      let buffer = '';
      const decoder = new TextDecoder();
      while (true) {
        const { value, done } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });
        let boundary = buffer.indexOf('\n');
        while (boundary !== -1) {
          const chunk = buffer.slice(0, boundary).trim();
          buffer = buffer.slice(boundary + 1);
          if (chunk) {
            const update = JSON.parse(chunk) as ChatResponseUpdateAppModel;
            this.messageSubject$.next(update);
          }
          boundary = buffer.indexOf('\n');
        }
      }
      // Handle any trailing chunk
      if (buffer.trim()) {
        const update = JSON.parse(buffer.trim()) as ChatResponseUpdateAppModel;
        this.messageSubject$.next(update);
      }
    } catch (err) {
      const errorMessage =
        typeof err === 'object' && err !== null && 'message' in err && typeof (err as any).message === 'string'
          ? (err as { message: string }).message
          : 'An error occurred while processing the chat response.';

      const errorUpdate: ChatResponseUpdateAppModel = {
        contents: [{
          $type: 'error',
          message: errorMessage
        }],
        role: ChatRoleEnumAppModel.Assistant,
        created_at: new Date().toISOString(),
        finish_reason: ChatFinishReasonAppModel.Stop,
      }
      console.error('ChatHttpStream error:', errorMessage, err);
      this.messageSubject$.next(errorUpdate);
    }
  }

  getMessages(): Observable<ChatResponseUpdateAppModel> {
    return this.messageSubject$.asObservable();
  }
}
