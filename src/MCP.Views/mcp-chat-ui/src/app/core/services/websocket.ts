import { Injectable, signal } from '@angular/core';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { LLMStreamResponse } from '../../shared/models/message.interface';

@Injectable({
  providedIn: 'root'
})
export class Websocket {
  private readonly messageSubject$ = new Subject<LLMStreamResponse>();
  private readonly connectionStatus$ = new BehaviorSubject<boolean>(false);

  readonly isConnected = signal(false);

  private webSocket: WebSocket | null = null;
  private readonly baseUrl = 'ws://localhost:5000/ws/chat'; // Adjust based on your ASP.NET Core backend

  constructor() {}

  connect(): Observable<boolean> {
    if (this.webSocket?.readyState === WebSocket.OPEN) {
      return this.connectionStatus$.asObservable();
    }

    this.webSocket = new WebSocket(this.baseUrl);

    this.webSocket.onopen = () => {
      this.isConnected.set(true);
      this.connectionStatus$.next(true);
    };

    this.webSocket.onmessage = (event) => {
      try {
        const response: LLMStreamResponse = JSON.parse(event.data);
        this.messageSubject$.next(response);
      } catch (error) {
        console.error('Error parsing WebSocket message:', error);
      }
    };

    this.webSocket.onclose = () => {
      this.isConnected.set(false);
      this.connectionStatus$.next(false);
    };

    this.webSocket.onerror = (error) => {
      console.error('WebSocket error:', error);
      this.isConnected.set(false);
      this.connectionStatus$.next(false);
    };

    return this.connectionStatus$.asObservable();
  }

  sendMessage(message: string): void {
    if (this.webSocket?.readyState === WebSocket.OPEN) {
      this.webSocket.send(JSON.stringify({ content: message }));
    }
  }

  getMessages(): Observable<LLMStreamResponse> {
    return this.messageSubject$.asObservable();
  }

  disconnect(): void {
    if (this.webSocket) {
      this.webSocket.close();
      this.webSocket = null;
    }
  }
}
