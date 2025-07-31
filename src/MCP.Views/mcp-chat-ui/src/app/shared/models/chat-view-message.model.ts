// Renamed from chat-message.model.ts for frontend view model clarity

// View model for chat messages used in the UI
export interface ChatMessageView {
  id: string;
  content: string;
  isUser: boolean;
  timestamp: Date;
  isStreaming?: boolean;
  isError?: boolean;
}
