// Auto-generated from OpenAPI spec (components/schemas/McpToolDescription)
export interface McpToolDescription {
  name: string;
  title?: string | null;
  description: string;
  input_schema?: unknown; // JSON Schema object
  output_schema?: unknown; // JSON Schema object
  version?: string | null;
  author?: string | null;
  tags?: string[] | null;
}

// Auto-generated from OpenAPI spec (components/schemas/McpClientToolRequest)
export interface McpClientToolRequest {
  server_name: string;
  tool_name: string;
  arguments?: Record<string, unknown> | null;
}

// Auto-generated from OpenAPI spec (components/schemas/McpClientRoleDescription)
export enum McpClientRoleDescription {
  Assistant = "assistant",
  Tool = "user"
}

// Auto-generated from OpenAPI spec (components/schemas/McpClientAnnotationsDescription)
export interface McpClientAnnotationsDescription {
  audience?: McpClientRoleDescription[] | null;
  priority?: number | null;
  lastModified?: string | null; // DateTimeOffset as ISO string
}

// Auto-generated from OpenAPI spec (components/schemas/McpClientToolContentBlock)
export interface McpClientToolContentBlockBase {
  $type: string;
  annotations?: McpClientAnnotationsDescription | null;
}

export interface McpClientToolTextContentBlock extends McpClientToolContentBlockBase {
  $type: 'text';
  text: string;
  _meta?: unknown | null;
}

export interface McpClientToolImageContentBlock extends McpClientToolContentBlockBase {
  $type: 'image';
  data: string;
  media_type: string;
  _meta?: unknown | null;
}

export interface McpClientToolAudioContentBlock extends McpClientToolContentBlockBase {
  $type: 'audio';
  data: string;
  media_type: string;
  _meta?: unknown | null;
}

// Auto-generated from OpenAPI spec (components/schemas/McpClientToolResourceContents)
export interface McpClientToolResourceContentsBase {
  $type: string;
  uri: string;
  media_type?: string | null;
  _meta?: unknown | null;
}

export interface McpClientToolBlobResourceContents extends McpClientToolResourceContentsBase {
  $type: 'blob';
  blob: string;
}

export interface McpClientToolTextResourceContents extends McpClientToolResourceContentsBase {
  $type: 'text';
  text: string;
}

export type McpClientToolResourceContents =
  | McpClientToolBlobResourceContents
  | McpClientToolTextResourceContents;

export interface McpClientToolEmbeddedResourceBlock extends McpClientToolContentBlockBase {
  $type: 'resource';
  resource: McpClientToolResourceContents;
  _meta?: unknown | null;
}

export interface McpClientToolResourceLinkBlock extends McpClientToolContentBlockBase {
  $type: 'resource_link';
  uri: string;
  name: string;
  description?: string | null;
  media_type?: string | null;
  size?: number | null;
}

export type McpClientToolContentBlock =
  | McpClientToolTextContentBlock
  | McpClientToolImageContentBlock
  | McpClientToolAudioContentBlock
  | McpClientToolEmbeddedResourceBlock
  | McpClientToolResourceLinkBlock;

// Auto-generated from OpenAPI spec (components/schemas/McpClientToolResponse)
export interface McpClientToolResponse {
  content: McpClientToolContentBlock[];
  structured_content?: unknown | null; // JsonNode
  isError?: boolean | null;
}

