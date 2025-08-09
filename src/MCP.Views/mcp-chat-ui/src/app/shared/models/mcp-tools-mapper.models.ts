import { McpToolDescription } from './mcp-tools-api.models';
import { McpToolDescriptionView } from './mcp-tools-view.models';

export function toMcpToolDescriptionView(api: McpToolDescription): McpToolDescriptionView {
  return {
    name: api.name,
    title: api.title ?? null,
    description: api.description,
    inputSchema: api.input_schema ?? null,
    outputSchema: api.output_schema ?? null,
    version: api.version ?? null,
    author: api.author ?? null,
    tags: api.tags ?? null
  };
}

export function fromMcpToolDescriptionView(view: McpToolDescriptionView): McpToolDescription {
  return {
    name: view.name,
    title: view.title ?? null,
    description: view.description,
    input_schema: view.inputSchema ?? null,
    output_schema: view.outputSchema ?? null,
    version: view.version ?? null,
    author: view.author ?? null,
    tags: view.tags ?? null
  };
}
