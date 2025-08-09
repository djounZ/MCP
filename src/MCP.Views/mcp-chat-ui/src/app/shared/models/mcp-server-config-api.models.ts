// Auto-generated from OpenAPI spec (components/schemas/McpServerConfigurationItem)
export interface McpServerConfigurationItem {
  category?: string | null;
  command?: string | null;
  args?: string[] | null;
  env?: { [key: string]: string } | null;
  url?: string | null;
  type?: 'stdio' | 'http' | null;
  [key: string]: any; // For any additional properties
}

