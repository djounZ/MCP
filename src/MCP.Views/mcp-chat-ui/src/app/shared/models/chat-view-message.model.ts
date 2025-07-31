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
  [key: string]: unknown;
}

// View model for ChatOptions (UI)
export interface ChatOptionsView {
  conversationId?: string | null;
  instructions?: string | null;
  temperature?: number | null;
  maxOutputTokens?: number | null;
  topP?: number | null;
  topK?: number | null;
  frequencyPenalty?: number | null;
  presencePenalty?: number | null;
  seed?: number | null;
  responseFormat?: unknown | null; // Use a more specific type if needed
  modelId?: string | null;
  stopSequences?: string[] | null;
  allowMultipleToolCalls?: boolean | null;
  toolMode?: unknown | null; // Use a more specific type if needed
  [key: string]: unknown;
}

// View model for ChatRequest (UI)
export interface ChatRequestView {
  messages: ChatMessageView[];
  options?: ChatOptionsView | null;
}
