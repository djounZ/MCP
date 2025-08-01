import { Injectable, signal } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ChatRequest, ChatResponseUpdateAppModel } from '../../shared/models/chat-completion-api.models';

@Injectable({
  providedIn: 'root'
})
export class ChatHttpStream {
  private readonly messageSubject$ = new Subject<ChatResponseUpdateAppModel>();

  private readonly apiUrl = 'http://localhost:5200/api/chat/completions/stream';

  async sendMessage(message: ChatRequest): Promise<void> {
    try {
      const response = await fetch(this.apiUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(message)
      });
      if (!response.body) {
        this.messageSubject$.error({ error: 'No response body' });
        return;
      }
      const reader = response.body.getReader();
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
            try {
              const update = JSON.parse(chunk) as ChatResponseUpdateAppModel;
              console.log('AI chunk:', update); // Debug print
              this.messageSubject$.next(update);
            } catch (e) {
              // Ignore parse errors for incomplete/in-between chunks
            }
          }
          boundary = buffer.indexOf('\n');
        }
      }
      // Handle any trailing chunk
      if (buffer.trim()) {
        try {
          const update = JSON.parse(buffer.trim()) as ChatResponseUpdateAppModel;
          console.log('AI chunk (trailing):', update);
            this.messageSubject$.next(update);
        } catch (e) {
          this.messageSubject$.error({ error: 'Failed to parse trailing chunk' });
        }
      }
    } catch (err) {
      this.messageSubject$.error({ error: 'Network error' });
    }
  }

  getMessages(): Observable<ChatResponseUpdateAppModel> {
    return this.messageSubject$.asObservable();
  }
}
