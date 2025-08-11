// Auto-generated from OpenAPI spec (components/schemas/McpClientDescription)
// See McpServerConfigurationItem.cs for reference

export interface McpClientToolDescription {
  name: string;
  description?: string | null;
  input_schema: any; // JSON Schema object
  output_schema?: any | null;
}

export interface McpClientPromptArgumentDescription {
  name: string;
  title?: string | null;
  description?: string | null;
  required?: boolean | null;
}

export interface McpClientPromptDescription {
  name: string;
  title?: string | null;
  description?: string | null;
  arguments?: McpClientPromptArgumentDescription[] | null;
  _meta?: any | null;
}

export interface McpClientResourceTemplateDescription {
  name: string;
  title?: string | null;
  uriTemplate?: string | null;
  description?: string | null;
  mimeType?: string | null;
  annotations?: McpClientAnnotationsDescription | null;
  _meta?: any | null;
}

export interface McpClientResourceDescription {
  name: string;
  title?: string | null;
  uri?: string | null;
  description?: string | null;
  mimeType?: string | null;
  annotations?: McpClientAnnotationsDescription | null;
  size?: number | null;
  _meta?: any | null;
}

export interface McpClientAnnotationsDescription {
  audience?: McpClientRoleDescription[] | null;
  priority?: number | null;
  lastModified?: string | null; // ISO date string
}

export type McpClientRoleDescription = 'assistant' | 'tool';

export interface McpClientDescription {
  tools: McpClientToolDescription[];
  prompts: McpClientPromptDescription[];
  resource_templates: McpClientResourceTemplateDescription[];
  resources: McpClientResourceDescription[];
}

// The API returns: IDictionary<string, McpClientDescription>
export type McpClientDescriptionDictionary = Record<string, McpClientDescription>;
