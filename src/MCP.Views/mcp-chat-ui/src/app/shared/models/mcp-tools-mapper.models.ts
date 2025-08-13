import {
  McpToolDescription,
  McpClientToolRequest,
  McpClientToolResponse,
  McpClientToolContentBlock,
  McpClientAnnotationsDescription,
  McpClientToolResourceContents,
  McpClientRoleDescription
} from './mcp-tools-api.models';
import {
  McpToolDescriptionView,
  McpClientToolRequestView,
  McpClientToolResponseView,
  McpClientToolContentBlockView,
  McpClientAnnotationsDescriptionView,
  McpClientToolResourceContentsView,
  McpClientRoleDescriptionView
} from './mcp-tools-view.models';

// Tool Description mappers
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

// Role description mappers
function toMcpClientRoleDescriptionView(api: McpClientRoleDescription): McpClientRoleDescriptionView {
  switch (api) {
    case McpClientRoleDescription.Assistant:
      return McpClientRoleDescriptionView.Assistant;
    case McpClientRoleDescription.Tool:
      return McpClientRoleDescriptionView.Tool;
    default:
      throw new Error(`Unknown role: ${api}`);
  }
}

function fromMcpClientRoleDescriptionView(view: McpClientRoleDescriptionView): McpClientRoleDescription {
  switch (view) {
    case McpClientRoleDescriptionView.Assistant:
      return McpClientRoleDescription.Assistant;
    case McpClientRoleDescriptionView.Tool:
      return McpClientRoleDescription.Tool;
    default:
      throw new Error(`Unknown role: ${view}`);
  }
}

// Annotations mappers
export function toMcpClientAnnotationsDescriptionView(api: McpClientAnnotationsDescription): McpClientAnnotationsDescriptionView {
  return {
    audience: api.audience ? api.audience.map(toMcpClientRoleDescriptionView) : null,
    priority: api.priority ?? null,
    lastModified: api.lastModified ?? null
  };
}

export function fromMcpClientAnnotationsDescriptionView(view: McpClientAnnotationsDescriptionView): McpClientAnnotationsDescription {
  return {
    audience: view.audience ? view.audience.map(fromMcpClientRoleDescriptionView) : null,
    priority: view.priority ?? null,
    lastModified: view.lastModified ?? null
  };
}

// Resource content mappers
export function toMcpClientToolResourceContentsView(api: McpClientToolResourceContents): McpClientToolResourceContentsView {
  const baseView = {
    type: api.$type,
    uri: api.uri,
    mediaType: api.media_type ?? null,
    meta: api._meta ?? null
  };

  switch (api.$type) {
    case 'blob':
      return {
        ...baseView,
        type: 'blob',
        blob: (api as any).blob
      };
    case 'text':
      return {
        ...baseView,
        type: 'text',
        text: (api as any).text
      };
    default:
      throw new Error(`Unknown resource content type: ${(api as any).$type}`);
  }
}

export function fromMcpClientToolResourceContentsView(view: McpClientToolResourceContentsView): McpClientToolResourceContents {
  const baseApi = {
    $type: view.type,
    uri: view.uri,
    media_type: view.mediaType ?? null,
    _meta: view.meta ?? null
  };

  switch (view.type) {
    case 'blob':
      return {
        ...baseApi,
        $type: 'blob',
        blob: (view as any).blob
      } as any;
    case 'text':
      return {
        ...baseApi,
        $type: 'text',
        text: (view as any).text
      } as any;
    default:
      throw new Error(`Unknown resource content type: ${(view as any).type}`);
  }
}

// Content block mappers
export function toMcpClientToolContentBlockView(api: McpClientToolContentBlock): McpClientToolContentBlockView {
  const baseView = {
    type: api.$type,
    annotations: api.annotations ? toMcpClientAnnotationsDescriptionView(api.annotations) : null
  };

  switch (api.$type) {
    case 'text':
      return {
        ...baseView,
        type: 'text',
        text: (api as any).text,
        meta: (api as any)._meta ?? null
      };
    case 'image':
      return {
        ...baseView,
        type: 'image',
        data: (api as any).data,
        mediaType: (api as any).media_type,
        meta: (api as any)._meta ?? null
      };
    case 'audio':
      return {
        ...baseView,
        type: 'audio',
        data: (api as any).data,
        mediaType: (api as any).media_type,
        meta: (api as any)._meta ?? null
      };
    case 'resource':
      return {
        ...baseView,
        type: 'resource',
        resource: toMcpClientToolResourceContentsView((api as any).resource),
        meta: (api as any)._meta ?? null
      };
    case 'resource_link':
      return {
        ...baseView,
        type: 'resource_link',
        uri: (api as any).uri,
        name: (api as any).name,
        description: (api as any).description ?? null,
        mediaType: (api as any).media_type ?? null,
        size: (api as any).size ?? null
      };
    default:
      throw new Error(`Unknown content block type: ${(api as any).$type}`);
  }
}

export function fromMcpClientToolContentBlockView(view: McpClientToolContentBlockView): McpClientToolContentBlock {
  const baseApi = {
    $type: view.type,
    annotations: view.annotations ? fromMcpClientAnnotationsDescriptionView(view.annotations) : null
  };

  switch (view.type) {
    case 'text':
      return {
        ...baseApi,
        $type: 'text',
        text: (view as any).text,
        _meta: (view as any).meta ?? null
      } as any;
    case 'image':
      return {
        ...baseApi,
        $type: 'image',
        data: (view as any).data,
        media_type: (view as any).mediaType,
        _meta: (view as any).meta ?? null
      } as any;
    case 'audio':
      return {
        ...baseApi,
        $type: 'audio',
        data: (view as any).data,
        media_type: (view as any).mediaType,
        _meta: (view as any).meta ?? null
      } as any;
    case 'resource':
      return {
        ...baseApi,
        $type: 'resource',
        resource: fromMcpClientToolResourceContentsView((view as any).resource),
        _meta: (view as any).meta ?? null
      } as any;
    case 'resource_link':
      return {
        ...baseApi,
        $type: 'resource_link',
        uri: (view as any).uri,
        name: (view as any).name,
        description: (view as any).description ?? null,
        media_type: (view as any).mediaType ?? null,
        size: (view as any).size ?? null
      } as any;
    default:
      throw new Error(`Unknown content block type: ${(view as any).type}`);
  }
}

// Tool Request mappers
export function toMcpClientToolRequestView(api: McpClientToolRequest): McpClientToolRequestView {
  return {
    serverName: api.server_name,
    toolName: api.tool_name,
    arguments: api.arguments ?? null
  };
}

export function fromMcpClientToolRequestView(view: McpClientToolRequestView): McpClientToolRequest {
  return {
    server_name: view.serverName,
    tool_name: view.toolName,
    arguments: view.arguments ?? null
  };
}

// Tool Response mappers
export function toMcpClientToolResponseView(api: McpClientToolResponse): McpClientToolResponseView {
  return {
    content: api.content.map(toMcpClientToolContentBlockView),
    structuredContent: api.structured_content ?? null,
    isError: api.isError ?? null
  };
}

export function fromMcpClientToolResponseView(view: McpClientToolResponseView): McpClientToolResponse {
  return {
    content: view.content.map(fromMcpClientToolContentBlockView),
    structured_content: view.structuredContent ?? null,
    isError: view.isError ?? null
  };
}
