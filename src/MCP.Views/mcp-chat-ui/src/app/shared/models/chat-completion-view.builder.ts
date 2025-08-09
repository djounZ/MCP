import { AiToolAppModel } from './chat-completion-api.models';
export class ChatOptionsAppModelViewBuilder {
  private _conversationId: string | null = null;
  private _instructions: string | null = null;
  private _temperature: number | null = null;
  private _maxOutputTokens: number | null = null;
  private _topP: number | null = null;
  private _topK: number | null = null;
  private _frequencyPenalty: number | null = null;
  private _presencePenalty: number | null = null;
  private _seed: number | null = null;
  private _responseFormat: ChatResponseFormatAppModelView | null = null;
  private _modelId: string | null = null;
  private _stopSequences: string[] | null = null;
  private _allowMultipleToolCalls: boolean | null = null;
  private _toolMode: ChatToolModeAppModelView | null = null;
  private _tools: Map<string, AiToolAppModel[]> | null = null;

  conversationId(conversationId: string | null): this {
    this._conversationId = conversationId;
    return this;
  }
  instructions(instructions: string | null): this {
    this._instructions = instructions;
    return this;
  }
  temperature(temperature: number | null): this {
    this._temperature = temperature;
    return this;
  }
  maxOutputTokens(maxOutputTokens: number | null): this {
    this._maxOutputTokens = maxOutputTokens;
    return this;
  }
  topP(topP: number | null): this {
    this._topP = topP;
    return this;
  }
  topK(topK: number | null): this {
    this._topK = topK;
    return this;
  }
  frequencyPenalty(frequencyPenalty: number | null): this {
    this._frequencyPenalty = frequencyPenalty;
    return this;
  }
  presencePenalty(presencePenalty: number | null): this {
    this._presencePenalty = presencePenalty;
    return this;
  }
  seed(seed: number | null): this {
    this._seed = seed;
    return this;
  }
  responseFormat(responseFormat: ChatResponseFormatAppModelView | null): this {
    this._responseFormat = responseFormat;
    return this;
  }
  modelId(modelId: string | null): this {
    this._modelId = modelId;
    return this;
  }
  stopSequences(stopSequences: string[] | null): this {
    this._stopSequences = stopSequences;
    return this;
  }
  allowMultipleToolCalls(allowMultipleToolCalls: boolean | null): this {
    this._allowMultipleToolCalls = allowMultipleToolCalls;
    return this;
  }
  toolMode(toolMode: ChatToolModeAppModelView | null): this {
    this._toolMode = toolMode;
    return this;
  }
  tools(tools: Map<string, AiToolAppModel[]> | null): this {
    this._tools = tools;
    return this;
  }

  build(): ChatOptionsAppModelView {
    return {
      conversationId: this._conversationId,
      instructions: this._instructions,
      temperature: this._temperature,
      maxOutputTokens: this._maxOutputTokens,
      topP: this._topP,
      topK: this._topK,
      frequencyPenalty: this._frequencyPenalty,
      presencePenalty: this._presencePenalty,
      seed: this._seed,
      responseFormat: this._responseFormat,
      modelId: this._modelId,
      stopSequences: this._stopSequences,
      allowMultipleToolCalls: this._allowMultipleToolCalls,
      toolMode: this._toolMode,
      tools: this._tools
    };
  }
}
export class AiContentAppModelTextContentAppModelViewBuilder {
  private _text: string | null = null;
  private _annotations: unknown[] | null = null;

  text(text: string): this {
    this._text = text;
    return this;
  }

  annotations(annotations: unknown[]): this {
    this._annotations = annotations;
    return this;
  }

  build(): any {
    return {
      $type: 'text',
      text: this._text,
      annotations: this._annotations
    };
  }
}
export class ChatRequestViewBuilder {
  private _messages: ChatMessageAppModelView[] = [];
  private _options: ChatOptionsAppModelView | null = null;
  private _provider: string | null = null;

  messages(messages: ChatMessageAppModelView[]): this {
    this._messages = messages;
    return this;
  }

  options(options: ChatOptionsAppModelView): this {
    this._options = options;
    return this;
  }

  provider(provider: string | null): this {
    this._provider = provider;
    return this;
  }

  build(): { messages: ChatMessageAppModelView[]; options: ChatOptionsAppModelView | null; provider: string | null } {
    return {
      messages: this._messages,
      options: this._options,
      provider: this._provider
    };
  }
}

import { ChatResponseAppModelView, ChatMessageAppModelView, ChatFinishReasonAppModelView, ChatRoleEnumAppModelView, AiContentAppModelView, ChatOptionsAppModelView, ChatResponseFormatAppModelView, ChatToolModeAppModelView } from './chat-completion-view.models';
import { generateUuid } from '../utils/uuid.utils';
export class ChatMessageAppModelViewBuilder {
  private _role: ChatRoleEnumAppModelView = ChatRoleEnumAppModelView.User;
  private _contents: AiContentAppModelView[] = [];
  private _messageTime: Date = new Date();

  role(role: ChatRoleEnumAppModelView): this {
    this._role = role;
    return this;
  }

  contents(contents: AiContentAppModelView[]): this {
    this._contents = contents;
    return this;
  }

  messageTime(messageTime: Date): this {
    this._messageTime = messageTime;
    return this;
  }

  build(): ChatMessageAppModelView {
    return {
      role: this._role,
      contents: this._contents,
      messageTime: this._messageTime
    };
  }
}

export class ChatResponseAppModelViewBuilder {
  private _messages: ChatMessageAppModelView[] = [];
  private _responseId: string | null = null;
  private _conversationId: string | null = generateUuid();
  private _modelId: string | null = null;
  private _createdAt: Date | null = new Date();
  private _finishReason: ChatFinishReasonAppModelView | null = null;

  messages(messages: ChatMessageAppModelView[]): this {
    this._messages = messages;
    return this;
  }

  responseId(responseId: string): this {
    this._responseId = responseId;
    return this;
  }

  conversationId(conversationId: string): this {
    this._conversationId = conversationId;
    return this;
  }

  modelId(modelId: string): this {
    this._modelId = modelId;
    return this;
  }

  createdAt(createdAt: Date): this {
    this._createdAt = createdAt;
    return this;
  }

  finishReason(finishReason: ChatFinishReasonAppModelView): this {
    this._finishReason = finishReason;
    return this;
  }

  build(): ChatResponseAppModelView {
    return {
      messages: this._messages,
      responseId: this._responseId,
      conversationId: this._conversationId,
      modelId: this._modelId,
      createdAt: this._createdAt,
      finishReason: this._finishReason
    };
  }
}
