export interface MessageContentTypeFilter {
    text: boolean;
    reasoning: boolean;
    error: boolean;
    functionCall: boolean;
    functionResult: boolean;
}

export interface MessageRoleFilter {
    user: boolean;
    assistant: boolean;
    tool: boolean;
    system: boolean;
}

export interface MessageFilters {
    roles: MessageRoleFilter;
    contentTypes: MessageContentTypeFilter;
}
