// Renamed from chat-message.model.ts for frontend view model clarity

// View model for chat messages used in the UI
export interface ChatResponseUpdateView {
  id: string;
  content: string;
  isUser: boolean;
  timestamp: Date;
  isStreaming?: boolean;
  isError?: boolean;
}

// View model for AIContent (UI)
export type AIContentView =
  | AIContentDataContentView
  | AIContentErrorContentView
  | AIContentFunctionCallContentView
  | AIContentFunctionResultContentView
  | AIContentTextContentView
  | AIContentTextReasoningContentView
  | AIContentUriContentView
  | AIContentUsageContentView
  | AIContentBaseView;

export interface AIContentBaseView {
  $type: string;
  additionalProperties?: Record<string, unknown> | null;
}

export interface AIContentDataContentView extends AIContentBaseView {
  $type: 'data';
  uri: string;
}

export interface AIContentErrorContentView extends AIContentBaseView {
  $type: 'error';
  message?: string | null;
  errorCode?: string | null;
  details?: string | null;
}

export interface AIContentFunctionCallContentView extends AIContentBaseView {
  $type: 'functionCall';
  callId: string;
  name: string;
  arguments?: Record<string, unknown> | null;
}

export interface AIContentFunctionResultContentView extends AIContentBaseView {
  $type: 'functionResult';
  callId: string;
  result: unknown;
}

export interface AIContentTextContentView extends AIContentBaseView {
  $type: 'text';
  text?: string | null;
}

export interface AIContentTextReasoningContentView extends AIContentBaseView {
  $type: 'reasoning';
  text?: string | null;
}

export interface AIContentUriContentView extends AIContentBaseView {
  $type: 'uri';
  uri: string;
  mediaType: string;
}

export interface AIContentUsageContentView extends AIContentBaseView {
  $type: 'usage';
  details: {
    inputTokenCount?: number | null;
    outputTokenCount?: number | null;
    totalTokenCount?: number | null;
    additionalCounts?: Record<string, number> | null;
  };
}

// View model for ChatMessage (UI)
export interface ChatMessageView {
  authorName?: string | null;
  role?: string | null;
  contents?: AIContentView[] | null;
  messageId?: string | null;
  // UI-specific fields
  isUser?: boolean;
  timestamp?: Date;
  [key: string]: unknown;
}
