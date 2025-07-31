export interface AIContentTextReasoningContent extends AIContentBase {
  $type: 'reasoning';
  Text?: string | null;
}

export interface AIContentUriContent extends AIContentBase {
  $type: 'uri';
  Uri: string;
  MediaType: string;
}

export interface AIContentUsageContent extends AIContentBase {
  $type: 'usage';
  Details: {
    InputTokenCount?: number | null;
    OutputTokenCount?: number | null;
    TotalTokenCount?: number | null;
    AdditionalCounts?: Record<string, number> | null;
  };
}
// Renamed from message.interface.ts for backend API model clarity

// OpenAPI: ChatResponseUpdate
export interface ChatResponseUpdate {
  AuthorName?: string | null;
  Role?: string | null; // NullableOfChatRole, can be string or null
  Contents?: AIContent[] | null;
  ResponseId?: string | null;
  MessageId?: string | null;
  ConversationId?: string | null;
  CreatedAt?: string | null; // ISO date-time string
  FinishReason?: string | null; // NullableOfChatFinishReason, can be string or null
  ModelId?: string | null;
  [key: string]: unknown; // For additionalProperties
}

// OpenAPI: AIContent (union type)
export type AIContent =
  | AIContentDataContent
  | AIContentErrorContent
  | AIContentFunctionCallContent
  | AIContentFunctionResultContent
  | AIContentTextContent
  | AIContentTextReasoningContent
  | AIContentUriContent
  | AIContentUsageContent
  | AIContentBase;

export interface AIContentBase {
  additionalProperties?: Record<string, unknown> | null;
}

export interface AIContentDataContent extends AIContentBase {
  $type: 'data';
  Uri: string;
}

export interface AIContentErrorContent extends AIContentBase {
  $type: 'error';
  Message?: string | null;
  ErrorCode?: string | null;
  Details?: string | null;
}

export interface AIContentFunctionCallContent extends AIContentBase {
  $type: 'functionCall';
  CallId: string;
  Name: string;
  Arguments?: Record<string, unknown> | null;
}

export interface AIContentFunctionResultContent extends AIContentBase {
  $type: 'functionResult';
  CallId: string;
  Result: unknown;
}

export interface AIContentTextContent extends AIContentBase {
  $type: 'text';
  Text?: string | null;
}

// OpenAPI: ChatMessage
export interface ChatMessage {
  authorName?: string | null;
  role?: string | null; // NullableOfChatRole
  contents?: AIContent[] | null;
  messageId?: string | null;
  [key: string]: unknown;
}

// OpenAPI: ChatOptions
export interface ChatOptions {
  conversationId?: string | null;
  instructions?: string | null;
  temperature?: number | null;
  maxOutputTokens?: number | null;
  topP?: number | null;
  topK?: number | null;
  frequencyPenalty?: number | null;
  presencePenalty?: number | null;
  seed?: number | null;
  responseFormat?: unknown | null; // ChatResponseFormat
  modelId?: string | null;
  stopSequences?: string[] | null;
  allowMultipleToolCalls?: boolean | null;
  toolMode?: unknown | null; // ChatToolMode
  [key: string]: unknown;
}

// OpenAPI: ChatRequest
export interface ChatRequest {
  messages: ChatMessage[];
  options?: ChatOptions | null;
}
