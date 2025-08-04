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

import { ChatResponseAppModelView, ChatMessageAppModelView, ChatFinishReasonAppModelView, ChatRoleEnumAppModelView, AiContentAppModelView, ChatOptionsAppModelView } from './chat-completion-view.models';
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
