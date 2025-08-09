// View model for MCP Tools
export interface McpToolDescriptionView {
  name: string;
  title?: string | null;
  description: string;
  inputSchema?: any; // JSON Schema object
  outputSchema?: any; // JSON Schema object
  version?: string | null;
  author?: string | null;
  tags?: string[] | null;
}

// View model for grouped tools
export interface McpToolGroupView {
  groupName: string;
  tools: McpToolDescriptionView[];
}
