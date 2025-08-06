// Auto-generated from OpenAPI spec (components/schemas/ChatRequest, ChatResponseAppModel)
// See backend.openapi.json for details

// --- ChatRoleEnumAppModel ---
export enum ChatRoleEnumAppModel {
  User = "user",
  Assistant = "assistant",
  System = "system",
  Tool = "tool"
}

// --- ChatFinishReasonAppModel ---
export enum ChatFinishReasonAppModel {
  Stop = "stop",
  Length = "length",
  ToolCalls = "tool_calls",
  ContentFilter = "content_filter"
}

// --- AiContentAppModel (union type, simplified) ---
export type AiContentAppModel =
  | AiContentAppModelDataContentAppModel
  | AiContentAppModelErrorContentAppModel
  | AiContentAppModelTextContentAppModel
  | AiContentAppModelTextReasoningContentAppModel
  | AiContentAppModelUriContentAppModel
  | AiContentAppModelUsageContentAppModel;

export interface AiContentAppModelDataContentAppModel {
  $type: 'data';
  uri: string;
  media_type?: string | null;
  name?: string | null;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelErrorContentAppModel {
  $type: 'error';
  message: string;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelTextContentAppModel {
  $type: 'text';
  text?: string | null;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelTextReasoningContentAppModel {
  $type: 'reasoning';
  text?: string | null;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelUriContentAppModel {
  $type: 'uri';
  uri: string;
  media_type: string;
  annotations?: unknown[] | null;
}
export interface AiContentAppModelUsageContentAppModel {
  $type: 'usage';
  details: {
    input_token_count?: number | null;
    output_token_count?: number | null;
    total_token_count?: number | null;
    additional_counts?: Record<string, number> | null;
  };
  annotations?: unknown[] | null;
}

// --- ChatMessageAppModel ---
export interface ChatMessageAppModel {
  role: ChatRoleEnumAppModel;
  contents: AiContentAppModel[];
}

// --- ChatToolModeAppModel ---
export type ChatToolModeAppModel =
  | ChatToolModeAppModelNoneChatToolModeAppModel
  | ChatToolModeAppModelAutoChatToolModeAppModel
  | ChatToolModeAppModelRequiredChatToolModeAppModel;

export interface ChatToolModeAppModelNoneChatToolModeAppModel {
  $type: 'none';
}
export interface ChatToolModeAppModelAutoChatToolModeAppModel {
  $type: 'auto';
}
export interface ChatToolModeAppModelRequiredChatToolModeAppModel {
  $type: 'required';
  required_function_name?: string | null;
}

// --- ChatResponseFormatAppModel ---
export type ChatResponseFormatAppModel =
  | ChatResponseFormatAppModelChatResponseFormatTextAppModel
  | ChatResponseFormatAppModelChatResponseFormatJsonAppModel;

export interface ChatResponseFormatAppModelChatResponseFormatTextAppModel {
  $type: 'text';
}
export interface ChatResponseFormatAppModelChatResponseFormatJsonAppModel {
  $type: 'json';
  schema?: string | null;
  schema_name?: string | null;
  schema_description?: string | null;
}

// --- ChatOptionsAppModel ---
export interface ChatOptionsAppModel {
  conversation_id?: string | null;
  instructions?: string | null;
  temperature?: number | null;
  maxOutputTokens?: number | null;
  top_p?: number | null;
  top_k?: number | null;
  frequency_penalty?: number | null;
  presence_penalty?: number | null;
  seed?: number | null;
  response_format?: ChatResponseFormatAppModel | null;
  model_id?: string | null;
  stop_sequences?: string[] | null;
  allow_multiple_tool_calls?: boolean | null;
  tool_mode?: ChatToolModeAppModel | null;
}

// --- ChatRequest ---
export interface ChatRequest {
  messages: ChatMessageAppModel[];
  options?: ChatOptionsAppModel | null;
  provider: string | null;
}

// --- ChatResponseAppModel ---
export interface ChatResponseAppModel {
  messages: ChatMessageAppModel[];
  response_id?: string | null;
  conversation_id?: string | null;
  model_id?: string | null;
  created_at?: string | null;
  finish_reason?: ChatFinishReasonAppModel | null;
}

// --- ChatResponseAppModel ---
export interface ChatResponseAppModel {
  messages: ChatMessageAppModel[];
  response_id?: string | null;
  conversation_id?: string | null;
  model_id?: string | null;
  created_at?: string | null;
  finish_reason?: ChatFinishReasonAppModel | null;
}
// --- ChatResponseUpdateAppModel ---
export interface ChatResponseUpdateAppModel {
  author_name?: string | null;
  role?: ChatRoleEnumAppModel | null;
  contents: AiContentAppModel[];
  response_id?: string | null;
  message_id?: string | null;
  conversation_id?: string | null;
  created_at?: string | null;
  finish_reason?: ChatFinishReasonAppModel | null;
  model_id?: string | null;
}

export interface AiProviderAiModelAppModel {
  id: string;
  name: string;
}

export interface AiProviderAppModel {
  name: string;
  models: AiProviderAiModelAppModel[];
}

