export function mapChatResponseUpdateToView(api: ChatResponseUpdate): ChatResponseUpdateView {
  return {
    authorName: api.AuthorName ?? null,
    role: api.Role ?? null,
    contents: api.Contents ? api.Contents.map(mapAIContentToAIContentView) : null,
    responseId: api.ResponseId ?? null,
    messageId: api.MessageId ?? null,
    conversationId: api.ConversationId ?? null,
    createdAt: api.CreatedAt ?? null,
    finishReason: api.FinishReason ?? null,
    modelId: api.ModelId ?? null,
    // Optionally add isStreaming/isError if you have logic for them
  };
}

function mapAIContentToAIContentView(api: AIContent): AIContentView {
  const type = (api as any).$type;
  switch (type) {
    case 'data':
      return {
        $type: 'data',
        uri: (api as any).Uri,
        additionalProperties: api.additionalProperties ?? null
      };
    case 'error':
      return {
        $type: 'error',
        message: (api as any).Message ?? null,
        errorCode: (api as any).ErrorCode ?? null,
        details: (api as any).Details ?? null,
        additionalProperties: api.additionalProperties ?? null
      };
    case 'functionCall':
      return {
        $type: 'functionCall',
        callId: (api as any).CallId,
        name: (api as any).Name,
        arguments: (api as any).Arguments ?? null,
        additionalProperties: api.additionalProperties ?? null
      };
    case 'functionResult':
      return {
        $type: 'functionResult',
        callId: (api as any).CallId,
        result: (api as any).Result,
        additionalProperties: api.additionalProperties ?? null
      };
    case 'text':
      return {
        $type: 'text',
        text: (api as any).Text ?? null,
        additionalProperties: api.additionalProperties ?? null
      };
    case 'reasoning':
      return {
        $type: 'reasoning',
        text: (api as any).Text ?? null,
        additionalProperties: api.additionalProperties ?? null
      };
    case 'uri':
      return {
        $type: 'uri',
        uri: (api as any).Uri,
        mediaType: (api as any).MediaType,
        additionalProperties: api.additionalProperties ?? null
      };
    case 'usage':
      return {
        $type: 'usage',
        details: (api as any).Details,
        additionalProperties: api.additionalProperties ?? null
      };
    default:
      return {
        $type: type,
        additionalProperties: api.additionalProperties ?? null
      };
  }
}
// Renamed from chat-message.mapper.ts for frontend view model mapping clarity

import { ChatResponseUpdate, AIContentTextContent, AIContentTextReasoningContent, AIContentErrorContent, ChatMessage, AIContent, AIContentDataContent, AIContentFunctionCallContent, AIContentFunctionResultContent, AIContentUriContent, AIContentUsageContent } from './chat-api.model';
import { ChatResponseUpdateView, ChatMessageView, AIContentView, ChatRequestView, ChatOptionsView } from './chat-view-message.model';
import { ChatRequest, ChatOptions } from './chat-api.model';

function isTextContent(c: unknown): c is AIContentTextContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'text';
}
function isReasoningContent(c: unknown): c is AIContentTextReasoningContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'reasoning';
}
function isErrorContent(c: unknown): c is AIContentErrorContent {
  return !!c && typeof c === 'object' && (c as any).$type === 'error';
}


export function mapChatMessageViewToChatMessage(view: ChatMessageView): ChatMessage {
  return {
    AuthorName: view.authorName ?? null,
    Role: view.role ?? null,
    Contents: view.contents ? view.contents.map(mapAIContentViewToAIContent) : null,
    MessageId: view.messageId ?? null,
    // Any additional properties
    ...Object.fromEntries(Object.entries(view).filter(([k]) => !['authorName','role','contents','messageId','isUser','timestamp'].includes(k)))
  };
}

function mapAIContentViewToAIContent(view: AIContentView): AIContent {
  switch (view.$type) {
    case 'data':
      return {
        $type: 'data',
        Uri: (view as any).uri,
        additionalProperties: view.additionalProperties ?? null
      } as AIContentDataContent;
    case 'error':
      return {
        $type: 'error',
        Message: (view as any).message ?? null,
        ErrorCode: (view as any).errorCode ?? null,
        Details: (view as any).details ?? null,
        additionalProperties: view.additionalProperties ?? null
      } as AIContentErrorContent;
    case 'functionCall':
      return {
        $type: 'functionCall',
        CallId: (view as any).callId,
        Name: (view as any).name,
        Arguments: (view as any).arguments ?? null,
        additionalProperties: view.additionalProperties ?? null
      } as AIContentFunctionCallContent;
    case 'functionResult':
      return {
        $type: 'functionResult',
        CallId: (view as any).callId,
        Result: (view as any).result,
        additionalProperties: view.additionalProperties ?? null
      } as AIContentFunctionResultContent;
    case 'text':
      return {
        $type: 'text',
        Text: (view as any).text ?? null,
        additionalProperties: view.additionalProperties ?? null
      } as AIContentTextContent;
    case 'reasoning':
      return {
        $type: 'reasoning',
        Text: (view as any).text ?? null,
        additionalProperties: view.additionalProperties ?? null
      } as AIContentTextReasoningContent;
    case 'uri':
      return {
        $type: 'uri',
        Uri: (view as any).uri,
        MediaType: (view as any).mediaType,
        additionalProperties: view.additionalProperties ?? null
      } as AIContentUriContent;
    case 'usage':
      return {
        $type: 'usage',
        Details: (view as any).details,
        additionalProperties: view.additionalProperties ?? null
      } as AIContentUsageContent;
    default:
      return {
        $type: (view as any).$type,
        additionalProperties: view.additionalProperties ?? null
      } as AIContent;
  }
}

export function mapChatRequestViewToChatRequest(view: ChatRequestView): ChatRequest {
  return {
    Messages: view.messages.map(mapChatMessageViewToChatMessage),
    Options: view.options ? mapChatOptionsViewToChatOptions(view.options) : undefined
  };
}

function mapChatOptionsViewToChatOptions(view: ChatOptionsView): ChatOptions {
  return {
    ConversationId: view.conversationId ?? null,
    Instructions: view.instructions ?? null,
    Temperature: view.temperature ?? null,
    MaxOutputTokens: view.maxOutputTokens ?? null,
    TopP: view.topP ?? null,
    TopK: view.topK ?? null,
    FrequencyPenalty: view.frequencyPenalty ?? null,
    PresencePenalty: view.presencePenalty ?? null,
    Seed: view.seed ?? null,
    ResponseFormat: view.responseFormat ?? null,
    ModelId: view.modelId ?? null,
    StopSequences: view.stopSequences ?? null,
    AllowMultipleToolCalls: view.allowMultipleToolCalls ?? null,
    ToolMode: view.toolMode ?? null,
    AdditionalProperties: view['additionalProperties'] ?? null
  };
}
