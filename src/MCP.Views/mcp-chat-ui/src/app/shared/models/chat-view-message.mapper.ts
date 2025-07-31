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

export function updateChatMessageView(
  prev: ChatResponseUpdateView,
  api: ChatResponseUpdate,
  opts?: { isStreaming?: boolean }
): void {
  let appendText = '';
  const content = api.Contents?.[0];
  if (content && (isTextContent(content) || isReasoningContent(content))) {
    appendText = content.Text || '';
  } else if (content && isErrorContent(content)) {
    appendText = content.Message || '';
    prev.isError = true;
  }
  prev.content += appendText;
  prev.isStreaming = opts?.isStreaming;
}

export function mapChatMessageViewToChatMessage(view: ChatMessageView): ChatMessage {
  return {
    authorName: view.authorName ?? null,
    role: view.role ?? null,
    contents: view.contents ? view.contents.map(mapAIContentViewToAIContent) : null,
    messageId: view.messageId ?? null,
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
    messages: view.messages.map(mapChatMessageViewToChatMessage),
    options: view.options ? mapChatOptionsViewToChatOptions(view.options) : undefined
  };
}

function mapChatOptionsViewToChatOptions(view: ChatOptionsView): ChatOptions {
  return {
    conversationId: view.conversationId ?? null,
    instructions: view.instructions ?? null,
    temperature: view.temperature ?? null,
    maxOutputTokens: view.maxOutputTokens ?? null,
    topP: view.topP ?? null,
    topK: view.topK ?? null,
    frequencyPenalty: view.frequencyPenalty ?? null,
    presencePenalty: view.presencePenalty ?? null,
    seed: view.seed ?? null,
    responseFormat: view.responseFormat ?? null,
    modelId: view.modelId ?? null,
    stopSequences: view.stopSequences ?? null,
    allowMultipleToolCalls: view.allowMultipleToolCalls ?? null,
    toolMode: view.toolMode ?? null,
    additionalProperties: view['additionalProperties'] ?? null
  };
}
