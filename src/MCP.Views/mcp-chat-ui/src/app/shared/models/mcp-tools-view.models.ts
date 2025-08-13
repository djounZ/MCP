// View model for MCP Tools
export interface McpToolDescriptionView {
  name: string;
  title?: string | null;
  description: string;
  inputSchema?: unknown; // JSON Schema object
  outputSchema?: unknown; // JSON Schema object
  version?: string | null;
  author?: string | null;
  tags?: string[] | null;
}

// View model for grouped tools
export interface McpToolGroupView {
  groupName: string;
  tools: McpToolDescriptionView[];
}

// View model for MCP client tool request
export interface McpClientToolRequestView {
  serverName: string;
  toolName: string;
  arguments?: Record<string, unknown> | null;
}

// View model for MCP client role (using same enum as API for consistency)
export enum McpClientRoleDescriptionView {
  Assistant = "assistant",
  Tool = "user"
}

// View model for MCP client annotations
export interface McpClientAnnotationsDescriptionView {
  audience?: McpClientRoleDescriptionView[] | null;
  priority?: number | null;
  lastModified?: string | null;
}

// View model for MCP client tool content blocks
export interface McpClientToolContentBlockViewBase {
  type: string;
  annotations?: McpClientAnnotationsDescriptionView | null;
}

export interface McpClientToolTextContentBlockView extends McpClientToolContentBlockViewBase {
  type: 'text';
  text: string;
  meta?: unknown | null;
}

export interface McpClientToolImageContentBlockView extends McpClientToolContentBlockViewBase {
  type: 'image';
  data: string;
  mediaType: string;
  meta?: unknown | null;
}

export interface McpClientToolAudioContentBlockView extends McpClientToolContentBlockViewBase {
  type: 'audio';
  data: string;
  mediaType: string;
  meta?: unknown | null;
}

// View model for MCP client tool resource contents
export interface McpClientToolResourceContentsViewBase {
  type: string;
  uri: string;
  mediaType?: string | null;
  meta?: unknown | null;
}

export interface McpClientToolBlobResourceContentsView extends McpClientToolResourceContentsViewBase {
  type: 'blob';
  blob: string;
}

export interface McpClientToolTextResourceContentsView extends McpClientToolResourceContentsViewBase {
  type: 'text';
  text: string;
}

export type McpClientToolResourceContentsView =
  | McpClientToolBlobResourceContentsView
  | McpClientToolTextResourceContentsView;

export interface McpClientToolEmbeddedResourceBlockView extends McpClientToolContentBlockViewBase {
  type: 'resource';
  resource: McpClientToolResourceContentsView;
  meta?: unknown | null;
}

export interface McpClientToolResourceLinkBlockView extends McpClientToolContentBlockViewBase {
  type: 'resource_link';
  uri: string;
  name: string;
  description?: string | null;
  mediaType?: string | null;
  size?: number | null;
}

export type McpClientToolContentBlockView =
  | McpClientToolTextContentBlockView
  | McpClientToolImageContentBlockView
  | McpClientToolAudioContentBlockView
  | McpClientToolEmbeddedResourceBlockView
  | McpClientToolResourceLinkBlockView;

// View model for MCP client tool response
export interface McpClientToolResponseView {
  content: McpClientToolContentBlockView[];
  structuredContent?: unknown | null;
  isError?: boolean | null;
}
