
// View models corresponding to chat-completion-api.models.ts


export enum ChatRoleEnumAppModelView {
  User = "user",
  Assistant = "assistant",
  System = "system",
  Tool = "tool"
}

export enum ChatFinishReasonAppModelView {
  Stop = "stop",
  Length = "length",
  ToolCalls = "tool_calls",
  ContentFilter = "content_filter"
}

export type AiContentAppModelView =
  | AiContentAppModelDataContentAppModelView
  | AiContentAppModelErrorContentAppModelView
  | AiContentAppModelTextContentAppModelView
  | AiContentAppModelTextReasoningContentAppModelView
  | AiContentAppModelUriContentAppModelView
  | AiContentAppModelUsageContentAppModelView;

export interface AiContentAppModelDataContentAppModelView {
  $type: 'data';
  uri: string;
  mediaType?: string | null;
  name?: string | null;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelErrorContentAppModelView {
  $type: 'error';
  message: string;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelTextContentAppModelView {
  $type: 'text';
  text?: string | null;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelTextReasoningContentAppModelView {
  $type: 'reasoning';
  text?: string | null;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelUriContentAppModelView {
  $type: 'uri';
  uri: string;
  mediaType: string;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelUsageContentAppModelView {
  $type: 'usage';
  details: {
    inputTokenCount?: number | null;
    outputTokenCount?: number | null;
    totalTokenCount?: number | null;
    additionalCounts?: Record<string, number> | null;
  };
  annotations?: unknown[] | null;
}

export interface ChatMessageAppModelView {
  role: ChatRoleEnumAppModelView;
  contents: AiContentAppModelView[];
  messageTime: Date;
}

export type ChatToolModeAppModelView =
  | ChatToolModeAppModelNoneChatToolModeAppModelView
  | ChatToolModeAppModelAutoChatToolModeAppModelView
  | ChatToolModeAppModelRequiredChatToolModeAppModelView;

export interface ChatToolModeAppModelNoneChatToolModeAppModelView {
  $type: 'none';
}
export interface ChatToolModeAppModelAutoChatToolModeAppModelView {
  $type: 'auto';
}
export interface ChatToolModeAppModelRequiredChatToolModeAppModelView {
  $type: 'required';
  requiredFunctionName?: string | null;
}

export type ChatResponseFormatAppModelView =
  | ChatResponseFormatAppModelChatResponseFormatTextAppModelView
  | ChatResponseFormatAppModelChatResponseFormatJsonAppModelView;

export interface ChatResponseFormatAppModelChatResponseFormatTextAppModelView {
  $type: 'text';
}
export interface ChatResponseFormatAppModelChatResponseFormatJsonAppModelView {
  $type: 'json';
  schema?: string | null;
  schemaName?: string | null;
  schemaDescription?: string | null;
}

export interface ChatOptionsAppModelView {
  conversationId?: string | null;
  instructions?: string | null;
  temperature?: number | null;
  maxOutputTokens?: number | null;
  topP?: number | null;
  topK?: number | null;
  frequencyPenalty?: number | null;
  presencePenalty?: number | null;
  seed?: number | null;
  responseFormat?: ChatResponseFormatAppModelView | null;
  modelId?: string | null;
  stopSequences?: string[] | null;
  allowMultipleToolCalls?: boolean | null;
  toolMode?: ChatToolModeAppModelView | null;
  tools?: Map<string, AiToolAppModelView[]> | null;
}
// View model for AiToolAppModel
export interface AiToolAppModelView {
  name: string;
}

export interface ChatRequestView {
  messages: ChatMessageAppModelView[];
  options?: ChatOptionsAppModelView | null;
  provider: string | null;
}

export interface ChatResponseAppModelView {
  messages: ChatMessageAppModelView[];
  responseId?: string | null;
  conversationId?: string | null;
  modelId?: string | null;
  createdAt?: Date | null;
  finishReason?: ChatFinishReasonAppModelView | null;
}
export interface ChatResponseUpdateAppModelView {
  authorName?: string | null;
  role?: ChatRoleEnumAppModelView | null;
  contents: AiContentAppModelView[];
  responseId?: string | null;
  messageId?: string | null;
  conversationId?: string | null;
  createdAt?: Date | null;
  finishReason?: ChatFinishReasonAppModelView | null;
  modelId?: string | null;
}


export interface AiProviderAiModelAppModelView {
  id: string;
  name: string;
}

export interface AiProviderAppModelView {
  name: string;
  models: AiProviderAiModelAppModelView[];
}

