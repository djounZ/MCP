# MCP Tools API Implementation

This implementation provides a complete TypeScript interface for calling MCP (Model Context Protocol) tools from the Angular frontend.

## Overview

The implementation follows the existing patterns in the codebase and includes:

1. **API Models** (`mcp-tools-api.models.ts`) - Direct mappings from the OpenAPI specification
2. **View Models** (`mcp-tools-view.models.ts`) - Frontend-friendly models with camelCase properties
3. **Mappers** (`mcp-tools-mapper.models.ts`) - Conversion functions between API and view models
4. **HTTP Client** (`mcp-tools-http-client.ts`) - Service for making API calls
5. **Higher-Level Service** (`mcp-tool-call.service.ts`) - Angular service with signals for reactive state management
6. **Demo Component** (`mcp-tool-call-demo.component.ts`) - Example usage component

## Usage

### Basic Usage

```typescript
import { McpToolsHttpClient } from "./core/services/mcp-tools-http-client";
import { McpClientToolRequest } from "./shared/models/mcp-tools-api.models";

// Direct HTTP client usage
const client = new McpToolsHttpClient();

const request: McpClientToolRequest = {
  server_name: "my-server",
  tool_name: "my-tool",
  arguments: { param1: "value1", param2: 42 },
};

const response = await client.callTool(request);
```

### Reactive Service Usage

```typescript
import { McpToolCallService } from './features/mcp/tools/mcp-tool-call.service';

// Inject the service
constructor(private toolCallService: McpToolCallService) {}

// Call a tool with reactive state management
async callTool() {
  const result = await this.toolCallService.callTool(
    'server-name',
    'tool-name',
    { message: 'Hello, world!' }
  );

  // Access reactive state
  console.log('Loading:', this.toolCallService.getIsLoading()());
  console.log('Last Response:', this.toolCallService.getLastResponse()());
  console.log('Last Error:', this.toolCallService.getLastError()());
}
```

### Component Usage

```typescript
import { McpToolCallDemoComponent } from "./features/mcp/tools/mcp-tool-call-demo.component";

// Use the demo component in your template
<app-mcp-tool-call-demo></app-mcp-tool-call-demo>;
```

## API Models

### McpClientToolRequest

```typescript
interface McpClientToolRequest {
  server_name: string;
  tool_name: string;
  arguments?: Record<string, unknown> | null;
}
```

### McpClientToolResponse

```typescript
interface McpClientToolResponse {
  content: McpClientToolContentBlock[];
  structured_content?: unknown | null;
  isError?: boolean | null;
}
```

### Content Block Types

The response supports multiple content block types with full type safety:

- **Text**: Plain text content
- **Image**: Base64 encoded image data with media type
- **Audio**: Base64 encoded audio data with media type
- **Resource**: Embedded resource content (blob or text) with structured typing
  - **Blob Resource**: Binary data encoded as base64
  - **Text Resource**: Plain text content
- **Resource Link**: URI links to external resources with metadata (name, description, media type, size)

## Error Handling

The implementation includes comprehensive error handling:

1. **Network Errors**: HTTP request failures
2. **Validation Errors**: Invalid JSON arguments
3. **Tool Errors**: Server-side tool execution errors
4. **Type Safety**: TypeScript compile-time checks with `unknown` types for better safety

## Type Safety

This implementation uses proper TypeScript interfaces instead of `unknown` where the structure is well-defined:

- **Polymorphic Content Blocks**: Discriminated unions for different content types
- **Resource Contents**: Structured types for blob and text resources
- **Tool Arguments**: `Record<string, unknown>` for flexible but safe parameter passing
- **JSON Schema Objects**: `unknown` for truly dynamic schemas
- **Metadata Fields**: `unknown` for extensible metadata

### Resource Content Types

```typescript
// Embedded resources are properly typed
interface McpClientToolResourceContents {
  $type: "blob" | "text";
  uri: string;
  media_type?: string | null;
  // Type-specific content
  blob?: string; // for blob resources
  text?: string; // for text resources
}

// Resource links include full metadata
interface McpClientToolResourceLinkBlock {
  $type: "resource_link";
  uri: string;
  name: string;
  description?: string | null;
  media_type?: string | null;
  size?: number | null;
}
```

### Enum Types

The implementation uses proper TypeScript enums for constrained values:

```typescript
// Role descriptions for annotation metadata
export enum McpClientRoleDescription {
  User = "user",
  Assistant = "assistant",
}

// Mapped to view layer with conversion functions
export enum McpClientRoleDescriptionView {
  User = "user",
  Assistant = "assistant",
}
```

This provides compile-time safety while maintaining flexibility for extensible fields.

## Testing

Run the included test file to verify the implementation:

```bash
# Run TypeScript compilation check
npm run build

# Run the test file (if transpiled)
node dist/test-mcp-tools.js
```

## Integration

To integrate this into your application:

1. Import the required services in your module or component
2. Use dependency injection to access the services
3. Call tools using either the HTTP client directly or the reactive service
4. Handle responses using the provided type-safe content extractors

## Example Integration

```typescript
// In your component
import { Component } from "@angular/core";
import { McpToolCallService } from "./features/mcp/tools/mcp-tool-call.service";

@Component({
  selector: "app-my-component",
  template: `
    <button (click)="testTool()">Test MCP Tool</button>
    @if (toolService.getLastResponse()(); as response) {
    <div>{{ response.content[0].text }}</div>
    }
  `,
})
export class MyComponent {
  constructor(private toolService: McpToolCallService) {}

  async testTool() {
    await this.toolService.callTool("weather-server", "get-weather", {
      location: "San Francisco",
      units: "metric",
    });
  }
}
```

## Future Enhancements

- Add retry logic for failed requests
- Implement caching for tool descriptions
- Add request/response logging
- Support for streaming responses
- Tool validation against input schemas
