// --- Array Mapper ---
export function toAiProviderAppModelViewArray(apiArray: AiProviderAppModel[]): AiProviderAppModelView[] {
  return Array.isArray(apiArray) ? apiArray.map(toAiProviderAppModelView) : [];
}

// --- AiTool Mappers ---
export function toAiToolAppModelView(api: AiToolAppModel): AiToolAppModelView {
  return {
    name: api.name
  };
}

export function fromAiToolAppModelView(view: AiToolAppModelView): AiToolAppModel {
  return {
    name: view.name
  };
}

export function toToolsMapView(apiTools: Map<string, AiToolAppModel[]> | null): Map<string, AiToolAppModelView[]> | null {
  if (!apiTools) return null;
  const viewTools = new Map<string, AiToolAppModelView[]>();
  for (const [server, tools] of apiTools.entries()) {
    viewTools.set(server, tools.map(toAiToolAppModelView));
  }
  return viewTools;
}

export function fromToolsMapView(viewTools: Map<string, AiToolAppModelView[]> | null): Map<string, AiToolAppModel[]> | null {
  if (!viewTools) return null;
  const apiTools = new Map<string, AiToolAppModel[]>();
  for (const [server, tools] of viewTools.entries()) {
    apiTools.set(server, tools.map(fromAiToolAppModelView));
  }
  return apiTools;
}

// --- AiProvider Mappers ---
export function toAiProviderAiModelAppModelView(api: AiProviderAiModelAppModel): AiProviderAiModelAppModelView {
  return {
    id: api.id,
    name: api.name
  };
}

export function fromAiProviderAiModelAppModelView(view: AiProviderAiModelAppModelView): AiProviderAiModelAppModel {
  return {
    id: view.id,
    name: view.name
  };
}

export function toAiProviderAppModelView(api: AiProviderAppModel): AiProviderAppModelView {
  return {
    name: api.name,
    models: Array.isArray(api.models) ? api.models.map(toAiProviderAiModelAppModelView) : []
  };
}

export function fromAiProviderAppModelView(view: AiProviderAppModelView): AiProviderAppModel {
  return {
    name: view.name,
    models: Array.isArray(view.models) ? view.models.map(fromAiProviderAiModelAppModelView) : []
  };
}
// Bi-directional mappers between API and View models for chat completion
// Auto-generated for ChatRequest, ChatResponseAppModel, and all nested types

import { from } from 'rxjs';
import {
  ChatRoleEnumAppModel,
  AiContentAppModel,
  AiContentAppModelDataContentAppModel,
  AiContentAppModelErrorContentAppModel,
  AiContentAppModelTextContentAppModel,
  AiContentAppModelTextReasoningContentAppModel,
  AiContentAppModelUriContentAppModel,
  AiContentAppModelUsageContentAppModel,
  ChatMessageAppModel,
  ChatToolModeAppModel,
  ChatToolModeAppModelNoneChatToolModeAppModel,
  ChatToolModeAppModelAutoChatToolModeAppModel,
  ChatToolModeAppModelRequiredChatToolModeAppModel,
  ChatResponseFormatAppModel,
  ChatResponseFormatAppModelChatResponseFormatTextAppModel,
  ChatResponseFormatAppModelChatResponseFormatJsonAppModel,
  ChatOptionsAppModel,
  ChatRequest,
  ChatResponseAppModel,
  ChatFinishReasonAppModel,
  ChatResponseUpdateAppModel,
  AiProviderAiModelAppModel,
  AiProviderAppModel,
  AiToolAppModel
} from './chat-completion-api.models';
import {
  ChatRoleEnumAppModelView,
  AiContentAppModelView,
  AiContentAppModelDataContentAppModelView,
  AiContentAppModelErrorContentAppModelView,
  AiContentAppModelTextContentAppModelView,
  AiContentAppModelTextReasoningContentAppModelView,
  AiContentAppModelUriContentAppModelView,
  AiContentAppModelUsageContentAppModelView,
  ChatMessageAppModelView,
  ChatToolModeAppModelView,
  ChatToolModeAppModelNoneChatToolModeAppModelView,
  ChatToolModeAppModelAutoChatToolModeAppModelView,
  ChatToolModeAppModelRequiredChatToolModeAppModelView,
  ChatResponseFormatAppModelView,
  ChatResponseFormatAppModelChatResponseFormatTextAppModelView,
  ChatResponseFormatAppModelChatResponseFormatJsonAppModelView,
  ChatOptionsAppModelView,
  ChatRequestView,
  ChatResponseAppModelView,
  ChatFinishReasonAppModelView,
  ChatResponseUpdateAppModelView,
  AiProviderAiModelAppModelView,
  AiProviderAppModelView,
  AiToolAppModelView
} from './chat-completion-view.models';

// --- Role Enum Mapper ---
export function toChatRoleEnumAppModelView(role: ChatRoleEnumAppModel): ChatRoleEnumAppModelView {
  switch (role) {
    case ChatRoleEnumAppModel.User: return ChatRoleEnumAppModelView.User;
    case ChatRoleEnumAppModel.Assistant: return ChatRoleEnumAppModelView.Assistant;
    case ChatRoleEnumAppModel.System: return ChatRoleEnumAppModelView.System;
    case ChatRoleEnumAppModel.Tool: return ChatRoleEnumAppModelView.Tool;
    default: throw new Error('Unknown ChatRoleEnumAppModel value: ' + role);
  }
}
export function fromChatRoleEnumAppModelView(role: ChatRoleEnumAppModelView): ChatRoleEnumAppModel {
  switch (role) {
    case ChatRoleEnumAppModelView.User: return ChatRoleEnumAppModel.User;
    case ChatRoleEnumAppModelView.Assistant: return ChatRoleEnumAppModel.Assistant;
    case ChatRoleEnumAppModelView.System: return ChatRoleEnumAppModel.System;
    case ChatRoleEnumAppModelView.Tool: return ChatRoleEnumAppModel.Tool;
    default: throw new Error('Unknown ChatRoleEnumAppModelView value: ' + role);
  }
}

// --- Role Enum Mapper ---
export function toChatFinishReasonAppModelView(role: ChatFinishReasonAppModel): ChatFinishReasonAppModelView {
  switch (role) {
    case ChatFinishReasonAppModel.Stop: return ChatFinishReasonAppModelView.Stop;
    case ChatFinishReasonAppModel.Length: return ChatFinishReasonAppModelView.Length;
    case ChatFinishReasonAppModel.ToolCalls: return ChatFinishReasonAppModelView.ToolCalls;
    case ChatFinishReasonAppModel.ContentFilter: return ChatFinishReasonAppModelView.ContentFilter;
    default: throw new Error('Unknown ChatFinishReasonAppModel value: ' + role);
  }
}
export function fromChatFinishReasonAppModelView(role: ChatFinishReasonAppModelView): ChatFinishReasonAppModel {
  switch (role) {
    case ChatFinishReasonAppModelView.Stop: return ChatFinishReasonAppModel.Stop;
    case ChatFinishReasonAppModelView.Length: return ChatFinishReasonAppModel.Length;
    case ChatFinishReasonAppModelView.ToolCalls: return ChatFinishReasonAppModel.ToolCalls;
    case ChatFinishReasonAppModelView.ContentFilter: return ChatFinishReasonAppModel.ContentFilter;
    default: throw new Error('Unknown ChatFinishReasonAppModelView value: ' + role);
  }
}
// --- AiContent Mappers ---
export function toAiContentAppModelView(api: AiContentAppModel): AiContentAppModelView {
  switch (api.$type) {
    case 'data':
      return {
        $type: 'data',
        uri: (api as AiContentAppModelDataContentAppModel).uri,
        mediaType: (api as AiContentAppModelDataContentAppModel).media_type ?? null,
        name: (api as AiContentAppModelDataContentAppModel).name ?? null,
        annotations: api.annotations ?? null
      };
    case 'error':
      return {
        $type: 'error',
        message: (api as AiContentAppModelErrorContentAppModel).message,
        annotations: api.annotations ?? null
      };
    case 'text':
      return {
        $type: 'text',
        text: (api as AiContentAppModelTextContentAppModel).text ?? null,
        annotations: api.annotations ?? null
      };
    case 'reasoning':
      return {
        $type: 'reasoning',
        text: (api as AiContentAppModelTextReasoningContentAppModel).text ?? null,
        annotations: api.annotations ?? null
      };
    case 'uri':
      return {
        $type: 'uri',
        uri: (api as AiContentAppModelUriContentAppModel).uri,
        mediaType: (api as AiContentAppModelUriContentAppModel).media_type,
        annotations: api.annotations ?? null
      };
    case 'usage':
      return {
        $type: 'usage',
        details: {
          inputTokenCount: (api as AiContentAppModelUsageContentAppModel).details.input_token_count ?? null,
          outputTokenCount: (api as AiContentAppModelUsageContentAppModel).details.output_token_count ?? null,
          totalTokenCount: (api as AiContentAppModelUsageContentAppModel).details.total_token_count ?? null,
          additionalCounts: (api as AiContentAppModelUsageContentAppModel).details.additional_counts ?? null
        },
        annotations: api.annotations ?? null
      };
    case 'function_call':
      return {
        $type: 'function_call',
        callId: (api as any).call_id,
        name: (api as any).name,
        arguments: (api as any).arguments ?? null,
        annotations: api.annotations ?? null
      };
    case 'function_result':
      return {
        $type: 'function_result',
        callId: (api as any).call_id,
        result: (api as any).result ?? null,
        annotations: api.annotations ?? null
      };
    default:
      throw new Error('Unknown AiContentAppModel type');
  }
}

export function fromAiContentAppModelView(view: AiContentAppModelView): AiContentAppModel {
  switch (view.$type) {
    case 'data':
      return {
        $type: 'data',
        uri: (view as AiContentAppModelDataContentAppModelView).uri,
        media_type: (view as AiContentAppModelDataContentAppModelView).mediaType ?? null,
        name: (view as AiContentAppModelDataContentAppModelView).name ?? null,
        annotations: view.annotations ?? null
      };
    case 'error':
      return {
        $type: 'error',
        message: (view as AiContentAppModelErrorContentAppModelView).message,
        annotations: view.annotations ?? null
      };
    case 'text':
      return {
        $type: 'text',
        text: (view as AiContentAppModelTextContentAppModelView).text ?? null,
        annotations: view.annotations ?? null
      };
    case 'reasoning':
      return {
        $type: 'reasoning',
        text: (view as AiContentAppModelTextReasoningContentAppModelView).text ?? null,
        annotations: view.annotations ?? null
      };
    case 'function_call':
      return {
        $type: 'function_call',
        call_id: (view as any).callId,
        name: (view as any).name,
        arguments: (view as any).arguments ?? null,
        annotations: view.annotations ?? null
      };
    case 'function_result':
      return {
        $type: 'function_result',
        call_id: (view as any).callId,
        result: (view as any).result ?? null,
        annotations: view.annotations ?? null
      };
    case 'uri':
      return {
        $type: 'uri',
        uri: (view as AiContentAppModelUriContentAppModelView).uri,
        media_type: (view as AiContentAppModelUriContentAppModelView).mediaType,
        annotations: view.annotations ?? null
      };
    case 'usage':
      return {
        $type: 'usage',
        details: {
          input_token_count: (view as AiContentAppModelUsageContentAppModelView).details.inputTokenCount ?? null,
          output_token_count: (view as AiContentAppModelUsageContentAppModelView).details.outputTokenCount ?? null,
          total_token_count: (view as AiContentAppModelUsageContentAppModelView).details.totalTokenCount ?? null,
          additional_counts: (view as AiContentAppModelUsageContentAppModelView).details.additionalCounts ?? null
        },
        annotations: view.annotations ?? null
      };
    default:
      throw new Error('Unknown AiContentAppModelView type');
  }
}

// --- ChatMessage Mappers ---
export function toChatMessageAppModelView(api: ChatMessageAppModel): ChatMessageAppModelView {
  return {
    role: toChatRoleEnumAppModelView(api.role),
    contents: api.contents.map(toAiContentAppModelView),
    messageTime: new Date()
  };
}
export function fromChatMessageAppModelView(view: ChatMessageAppModelView): ChatMessageAppModel {
  return {
    role: fromChatRoleEnumAppModelView(view.role),
    contents: view.contents.map(fromAiContentAppModelView)
  };
}

// --- ChatToolMode Mappers ---
export function toChatToolModeAppModelView(api: ChatToolModeAppModel | null): ChatToolModeAppModelView | null {
  if (!api) return null;
  switch (api.$type) {
    case 'none':
      return { $type: 'none' };
    case 'auto':
      return { $type: 'auto' };
    case 'required':
      return { $type: 'required', requiredFunctionName: (api as ChatToolModeAppModelRequiredChatToolModeAppModel).required_function_name ?? null };
    default:
      throw new Error('Unknown ChatToolModeAppModel type');
  }
}
export function fromChatToolModeAppModelView(view: ChatToolModeAppModelView | null): ChatToolModeAppModel | null {
  if (!view) return null;
  switch (view.$type) {
    case 'none':
      return { $type: 'none' };
    case 'auto':
      return { $type: 'auto' };
    case 'required':
      return { $type: 'required', required_function_name: (view as ChatToolModeAppModelRequiredChatToolModeAppModelView).requiredFunctionName ?? null };
    default:
      throw new Error('Unknown ChatToolModeAppModelView type');
  }
}

// --- ChatResponseFormat Mappers ---
export function toChatResponseFormatAppModelView(api: ChatResponseFormatAppModel | null): ChatResponseFormatAppModelView | null {
  if (!api) return null;
  switch (api.$type) {
    case 'text':
      return { $type: 'text' };
    case 'json':
      return {
        $type: 'json',
        schema: (api as ChatResponseFormatAppModelChatResponseFormatJsonAppModel).schema,
        schemaName: (api as ChatResponseFormatAppModelChatResponseFormatJsonAppModel).schema_name ?? null,
        schemaDescription: (api as ChatResponseFormatAppModelChatResponseFormatJsonAppModel).schema_description ?? null
      };
    default:
      throw new Error('Unknown ChatResponseFormatAppModel type');
  }
}
export function fromChatResponseFormatAppModelView(view: ChatResponseFormatAppModelView | null): ChatResponseFormatAppModel | null {
  if (!view) return null;
  switch (view.$type) {
    case 'text':
      return { $type: 'text' };
    case 'json':
      return {
        $type: 'json',
        schema: (view as ChatResponseFormatAppModelChatResponseFormatJsonAppModelView).schema,
        schema_name: (view as ChatResponseFormatAppModelChatResponseFormatJsonAppModelView).schemaName ?? null,
        schema_description: (view as ChatResponseFormatAppModelChatResponseFormatJsonAppModelView).schemaDescription ?? null
      };
    default:
      throw new Error('Unknown ChatResponseFormatAppModelView type');
  }
}

// --- ChatOptions Mappers ---
export function toChatOptionsAppModelView(api: ChatOptionsAppModel | null): ChatOptionsAppModelView | null {
  if (!api) return null;
  return {
    conversationId: api.conversation_id ?? null,
    instructions: api.instructions ?? null,
    temperature: api.temperature ?? null,
    maxOutputTokens: api.maxOutputTokens ?? null,
    topP: api.top_p ?? null,
    topK: api.top_k ?? null,
    frequencyPenalty: api.frequency_penalty ?? null,
    presencePenalty: api.presence_penalty ?? null,
    seed: api.seed ?? null,
    responseFormat: toChatResponseFormatAppModelView(api.response_format ?? null),
    modelId: api.model_id ?? null,
    stopSequences: api.stop_sequences ?? null,
    allowMultipleToolCalls: api.allow_multiple_tool_calls ?? null,
    toolMode: toChatToolModeAppModelView(api.tool_mode ?? null),
    tools: toToolsMapView(api.tools ?? null)
  };
}
export function fromChatOptionsAppModelView(view: ChatOptionsAppModelView | null, conversationId: string | null | undefined): ChatOptionsAppModel | null {
  if (!view) return null;
  return {
    conversation_id: view.conversationId ?? conversationId ?? null,
    instructions: view.instructions ?? null,
    temperature: view.temperature ?? null,
    maxOutputTokens: view.maxOutputTokens ?? null,
    top_p: view.topP ?? null,
    top_k: view.topK ?? null,
    frequency_penalty: view.frequencyPenalty ?? null,
    presence_penalty: view.presencePenalty ?? null,
    seed: view.seed ?? null,
    response_format: fromChatResponseFormatAppModelView(view.responseFormat ?? null),
    model_id: view.modelId ?? null,
    stop_sequences: view.stopSequences ?? null,
    allow_multiple_tool_calls: view.allowMultipleToolCalls ?? null,
    tool_mode: fromChatToolModeAppModelView(view.toolMode ?? null),
    tools: fromToolsMapView(view.tools ?? null)
  };
}

// --- ChatRequest Mappers ---
export function toChatRequestView(api: ChatRequest): ChatRequestView {
  return {
    messages: api.messages.map(toChatMessageAppModelView),
    options: toChatOptionsAppModelView(api.options ?? null),
    provider: api.provider
  };
}
export function fromChatRequestView(view: ChatRequestView, conversationId: string | null | undefined): ChatRequest {
  return {
    messages: view.messages.map(fromChatMessageAppModelView),
    options: fromChatOptionsAppModelView(view.options ?? null, conversationId),
    provider: view.provider
  };
}

// --- ChatResponseAppModel Mappers ---
export function toChatResponseAppModelView(api: ChatResponseAppModel): ChatResponseAppModelView {
  return {
    messages: api.messages.map(toChatMessageAppModelView),
    responseId: api.response_id ?? null,
    conversationId: api.conversation_id ?? null,
    modelId: api.model_id ?? null,
    createdAt: api.created_at ? new Date(api.created_at) : null,
    finishReason: toChatFinishReasonAppModelView(api.finish_reason ?? ChatFinishReasonAppModel.Stop)
  };
}
export function fromChatResponseAppModelView(view: ChatResponseAppModelView): ChatResponseAppModel {
  return {
    messages: view.messages.map(fromChatMessageAppModelView),
    response_id: view.responseId ?? null,
    conversation_id: view.conversationId ?? null,
    model_id: view.modelId ?? null,
    created_at: view.createdAt ? view.createdAt.toISOString() : null,
    finish_reason: fromChatFinishReasonAppModelView(view.finishReason ?? ChatFinishReasonAppModelView.Stop) ?? null
  };
}
export function toChatResponseUpdateAppModelView(api: ChatResponseUpdateAppModel): ChatResponseUpdateAppModelView {
  return {
    authorName: api.author_name ?? null,
    role: api.role !== undefined && api.role !== null ? toChatRoleEnumAppModelView(api.role) : null,
    contents: Array.isArray(api.contents) ? api.contents.map(toAiContentAppModelView) : [],
    responseId: api.response_id ?? null,
    messageId: api.message_id ?? null,
    conversationId: api.conversation_id ?? null,
    createdAt: api.created_at ? new Date(api.created_at) : null,
    finishReason: api.finish_reason !== undefined && api.finish_reason !== null ? toChatFinishReasonAppModelView(api.finish_reason) : null,
    modelId: api.model_id ?? null,
  };
}

export function fromChatResponseUpdateAppModelView(view: ChatResponseUpdateAppModelView): ChatResponseUpdateAppModel {
  return {
    author_name: view.authorName ?? null,
    role: view.role !== undefined && view.role !== null ? fromChatRoleEnumAppModelView(view.role) : null,
    contents: Array.isArray(view.contents) ? view.contents.map(fromAiContentAppModelView) : [],
    response_id: view.responseId ?? null,
    message_id: view.messageId ?? null,
    conversation_id: view.conversationId ?? null,
    created_at: view.createdAt ? view.createdAt.toISOString() : null,
    finish_reason: view.finishReason !== undefined && view.finishReason !== null ? fromChatFinishReasonAppModelView(view.finishReason) : null,
    model_id: view.modelId ?? null,
  };
}

export function fromChatResponseUpdateAppModelToChatResponseAppModelView(api: ChatResponseUpdateAppModel): ChatResponseAppModelView {
  return {
    messages: api.contents ? [{
      role: api.role !== undefined && api.role !== null
        ? (api.role as unknown as ChatRoleEnumAppModelView)
        : ChatRoleEnumAppModelView.Assistant,
      contents: api.contents as AiContentAppModelView[],
      messageTime: api.created_at ? new Date(api.created_at) : new Date()
    }] : [],
    responseId: api.response_id ?? null,
    conversationId: api.conversation_id ?? null,
    modelId: api.model_id ?? null,
    createdAt: api.created_at ? new Date(api.created_at) : null,
    finishReason: api.finish_reason !== undefined && api.finish_reason !== null ? (api.finish_reason as unknown as ChatFinishReasonAppModelView) : null
  };
}

export function updateChatResponseAppModelViewFromChatResponseUpdateAppModel(view: ChatResponseAppModelView, api: ChatResponseUpdateAppModel): void {
  if (!view || !api) return;
  // Append new message to messages array
  if (api.contents) {
    const viewContents = Array.isArray(api.contents) ? api.contents.map(toAiContentAppModelView) : [];
    const viewRole = api.role !== undefined && api.role !== null
      ? (api.role as unknown as ChatRoleEnumAppModelView)
      : ChatRoleEnumAppModelView.Assistant;
    const apiCreatedAt = api.created_at ?? null;
    updateChatMessageAppModelViewsFromAppModelContents(view.messages, viewRole, viewContents, apiCreatedAt);
  }
  view.responseId = api.response_id ?? view.responseId;
  view.conversationId = api.conversation_id ?? view.conversationId;
  view.modelId = api.model_id ?? view.modelId;
  view.createdAt = api.created_at ? new Date(api.created_at) : view.createdAt;
  view.finishReason = api.finish_reason !== undefined && api.finish_reason !== null
    ? (api.finish_reason as unknown as ChatFinishReasonAppModelView)
    : view.finishReason;
}

export function updateChatMessageAppModelViewsFromAppModelContents(viewMessages: ChatMessageAppModelView[], viewRole: ChatRoleEnumAppModelView, viewContents: AiContentAppModelView[], apiCreatedAt: string | null) {
  const idx = viewMessages.length - 1;
  if (idx < 0) {
    viewMessages.push({
      role: viewRole,
      contents: viewContents,
      messageTime: apiCreatedAt ? new Date(apiCreatedAt) : new Date()
    });
    return;
  }

  const last_message = viewMessages[idx];
  if (last_message && last_message.role === viewRole) {
    // Replace the entire message object to ensure signal reactivity
    const updatedMessage: ChatMessageAppModelView = {
      ...last_message,
      contents: [...last_message.contents, ...viewContents],
      messageTime: apiCreatedAt ? new Date(apiCreatedAt) : last_message.messageTime
    };
    viewMessages[idx] = updatedMessage;
  }
  else {
    viewMessages.push({
      role: viewRole,
      contents: viewContents,
      messageTime: apiCreatedAt ? new Date(apiCreatedAt) : new Date()
    });
  }
}

