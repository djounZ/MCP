export interface ChatMessage {
  id: string;
  content: string;
  isUser: boolean;
  timestamp: Date;
  isStreaming?: boolean;
  isError?: boolean;
}

export interface LLMStreamResponse {
  id: string;
  content: string;
  isComplete: boolean;
  error?: string;
}
