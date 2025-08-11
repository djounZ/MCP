// View model for MCP Client Description
export interface McpClientToolDescriptionView {
    name: string;
    description?: string | null;
    inputSchema: any;
    outputSchema?: any | null;
}

export interface McpClientPromptArgumentDescriptionView {
    name: string;
    title?: string | null;
    description?: string | null;
    required?: boolean | null;
}

export interface McpClientPromptDescriptionView {
    name: string;
    title?: string | null;
    description?: string | null;
    arguments?: McpClientPromptArgumentDescriptionView[] | null;
    meta?: any | null;
}

export interface McpClientResourceTemplateDescriptionView {
    name: string;
    title?: string | null;
    uriTemplate?: string | null;
    description?: string | null;
    mimeType?: string | null;
    annotations?: McpClientAnnotationsDescriptionView | null;
    meta?: any | null;
}

export interface McpClientResourceDescriptionView {
    name: string;
    title?: string | null;
    uri?: string | null;
    description?: string | null;
    mimeType?: string | null;
    annotations?: McpClientAnnotationsDescriptionView | null;
    size?: number | null;
    meta?: any | null;
}

export interface McpClientAnnotationsDescriptionView {
    audience?: McpClientRoleDescriptionView[] | null;
    priority?: number | null;
    lastModified?: string | null;
}

export type McpClientRoleDescriptionView = 'assistant' | 'tool';

export interface McpClientDescriptionView {
    tools: McpClientToolDescriptionView[];
    prompts: McpClientPromptDescriptionView[];
    resourceTemplates: McpClientResourceTemplateDescriptionView[];
    resources: McpClientResourceDescriptionView[];
}

// View model for dictionary
export type McpClientDescriptionDictionaryView = Record<string, McpClientDescriptionView>;
