// View model for MCP Server Configuration
export interface McpServerConfigurationView {
  category?: string | null;
  command?: string | null;
  args?: string[] | null;
  env?: { [key: string]: string } | null;
  url?: string | null;
  type?: 'stdio' | 'http' | null;
  // Add any view-specific fields here
}
