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
  private _options: any = null;

  messages(messages: ChatMessageAppModelView[]): this {
    this._messages = messages;
    return this;
  }

  options(options: any): this {
    this._options = options;
    return this;
  }

  build(): any {
    return {
      messages: this._messages,
      options: this._options
    };
  }
}

import { ChatResponseAppModelView, ChatMessageAppModelView, ChatFinishReasonAppModelView, ChatRoleEnumAppModelView, AiContentAppModelView } from './chat-completion-view.models';
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
  private _createdAt: string | null = new Date().toISOString();
  private _finishReason: ChatFinishReasonAppModelView | null = null;

  messages(messages: ChatMessageAppModelView[]): this {
    this._messages = messages;
    return this;
  }

  responseId(responseId: string | null): this {
    this._responseId = responseId;
    return this;
  }

  conversationId(conversationId: string | null): this {
    this._conversationId = conversationId;
    return this;
  }

  modelId(modelId: string | null): this {
    this._modelId = modelId;
    return this;
  }

  createdAt(createdAt: string | null): this {
    this._createdAt = createdAt;
    return this;
  }

  finishReason(finishReason: ChatFinishReasonAppModelView | null): this {
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
