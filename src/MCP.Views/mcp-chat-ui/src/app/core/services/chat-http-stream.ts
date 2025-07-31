import { Injectable, signal } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ChatResponseUpdate } from '../../shared/models/chat-api.model';

@Injectable({
  providedIn: 'root'
})
export class ChatHttpStream {
  private readonly messageSubject$ = new Subject<ChatResponseUpdate>();

  private readonly apiUrl = 'http://localhost:5200/api/chat/stream';

  connect(): Observable<boolean> {
    // For HTTP, always consider as connected
    return new Observable<boolean>(observer => {
      observer.next(true);
      observer.complete();
    });
  }

  async sendMessage(message: string): Promise<void> {
    try {
      const response = await fetch(this.apiUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Message: message })
      });
      if (!response.body) {
        this.messageSubject$.next({ id: '', content: '', isComplete: true, error: 'No response body' });
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
              const update = JSON.parse(chunk) as ChatResponseUpdate;
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
          const update = JSON.parse(buffer.trim());
          console.log('AI chunk (trailing):', update);
          let content = '';
          let error: string | undefined = undefined;
          let found = false;
          if (Array.isArray(update.contents) && update.contents.length > 0) {
            const c = update.contents[0];
            if (c.$type === 'text' || c.$type === 'reasoning') {
              content = c.text || '';
              found = true;
            } else if (c.$type === 'error') {
              error = c.message || 'Unknown error';
              found = true;
            }
          }
          if (found) {
            const mapped = {
              id: update.responseId || update.messageId || '',
              content,
              isComplete: !!update.finishReason,
              error
            };
            this.messageSubject$.next(mapped);
          } else {
            // Only log unrecognized trailing chunk
            console.warn('Unrecognized trailing AI chunk (not text/reasoning/error):', update);
          }
        } catch (e) {}
      }
    } catch (err) {
      this.messageSubject$.next({ id: '', content: '', isComplete: true, error: 'Network error' });
    }
  }

  getMessages(): Observable<ChatResponseUpdate> {
    return this.messageSubject$.asObservable();
  }

  disconnect(): void {
    // No-op for HTTP
  }
}
