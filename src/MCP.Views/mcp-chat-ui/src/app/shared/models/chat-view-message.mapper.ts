// Renamed from chat-message.mapper.ts for frontend view model mapping clarity

import { ChatResponseUpdate, AIContentTextContent, AIContentTextReasoningContent, AIContentErrorContent } from './chat-api.model';
import { ChatMessageView } from './chat-view-message.model';

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
  prev: ChatMessageView,
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
