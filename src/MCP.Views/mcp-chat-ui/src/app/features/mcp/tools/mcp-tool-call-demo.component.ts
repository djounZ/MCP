import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { JsonPipe } from '@angular/common';
import { McpToolCallService } from './mcp-tool-call.service';
import { McpClientToolContentBlockView } from '../../../shared/models/mcp-tools-view.models';

@Component({
  selector: 'app-mcp-tool-call-demo',
  imports: [FormsModule, JsonPipe],
  template: `
    <section class="mcp-tool-call-demo">
      <h2>MCP Tool Call Demo</h2>

      <div class="tool-call-form">
        <div class="form-group">
          <label for="serverName">Server Name:</label>
          <input
            id="serverName"
            type="text"
            [(ngModel)]="serverName"
            placeholder="Enter server name"
          />
        </div>

        <div class="form-group">
          <label for="toolName">Tool Name:</label>
          <input
            id="toolName"
            type="text"
            [(ngModel)]="toolName"
            placeholder="Enter tool name"
          />
        </div>

        <div class="form-group">
          <label for="arguments">Arguments (JSON):</label>
          <textarea
            id="arguments"
            [(ngModel)]="argumentsJson"
            placeholder='{"param1": "value1", "param2": 42}'
            rows="3"
          ></textarea>
        </div>

        <button
          (click)="callTool()"
          [disabled]="toolCallService.getIsLoading()()"
          class="call-button"
        >
          @if (toolCallService.getIsLoading()()) {
            Calling Tool...
          } @else {
            Call Tool
          }
        </button>
      </div>

      @if (toolCallService.getLastError()()) {
        <div class="error-message">
          <h3>Error:</h3>
          <p>{{ toolCallService.getLastError()() }}</p>
        </div>
      }

      @if (toolCallService.getLastResponse()(); as response) {
        <div class="response-display">
          <h3>Response:</h3>
          @if (response.isError) {
            <div class="error-response">
              <strong>Tool returned an error:</strong>
            </div>
          }

          <div class="content-blocks">
            @for (block of response.content; track block.type) {
              <div class="content-block" [attr.data-type]="block.type">
                @switch (block.type) {
                  @case ('text') {
                    <div class="text-content">
                      <h4>Text Content:</h4>
                      <pre>{{ getTextContent(block) }}</pre>
                    </div>
                  }
                  @case ('image') {
                      <div class="image-content">
                        @if (getImageMediaType(block) === 'image/png') {
                          <div class="image-preview">
                            <img
                              [src]="'data:image/png;base64,' + getImageData(block)"
                              alt="PNG Preview"
                              style="max-width: 300px; max-height: 200px; border: 1px solid #ccc; margin-top: 8px;"
                            />
                          </div>
                        }
                        <h4>Image Content:</h4>
                        <p>Media Type: {{ getImageMediaType(block) }}</p>
                        <p>Data Length: {{ getImageData(block).length || 0 }} characters</p>
                      </div>
                  }
                  @case ('audio') {
                    <div class="audio-content">
                      <h4>Audio Content:</h4>
                      <p>Media Type: {{ getAudioMediaType(block) }}</p>
                      <p>Data Length: {{ getAudioData(block).length || 0 }} characters</p>
                    </div>
                  }
                  @case ('resource') {
                    <div class="resource-content">
                      <h4>Resource Content:</h4>
                      <div class="resource-details">
                        <p>Type: {{ getResourceType(block) }}</p>
                        <p>URI: {{ getResourceUri(block) }}</p>
                        @if (getResourceMediaType(block)) {
                          <p>Media Type: {{ getResourceMediaType(block) }}</p>
                        }
                        @if (getResourceType(block) === 'text') {
                          <div class="resource-text-content">
                            <h5>Text Content:</h5>
                            <pre>{{ getResourceTextContent(block) }}</pre>
                          </div>
                        }
                        @if (getResourceType(block) === 'blob') {
                          <div class="resource-blob-content">
                            <h5>Blob Data:</h5>
                            <p>Length: {{ getResourceBlobContent(block).length || 0 }} characters</p>
                          </div>
                        }
                      </div>
                    </div>
                  }
                  @case ('resource_link') {
                    <div class="resource-link-content">
                      <h4>Resource Link:</h4>
                      <div class="resource-link-details">
                        <p>Name: {{ getResourceLinkName(block) }}</p>
                        @if (getResourceLinkDescription(block)) {
                          <p>Description: {{ getResourceLinkDescription(block) }}</p>
                        }
                        <p>URI: <a [href]="getResourceLinkUri(block)" target="_blank">{{ getResourceLinkUri(block) }}</a></p>
                        @if (getResourceLinkMediaType(block)) {
                          <p>Media Type: {{ getResourceLinkMediaType(block) }}</p>
                        }
                        @if (getResourceLinkSize(block)) {
                          <p>Size: {{ getResourceLinkSize(block) }} bytes</p>
                        }
                      </div>
                    </div>
                  }
                }
              </div>
            }
          </div>

          @if (response.structuredContent) {
            <div class="structured-content">
              <h4>Structured Content:</h4>
              <pre>{{ response.structuredContent | json }}</pre>
            </div>
          }
        </div>
      }
    </section>
  `,
  styles: [`
    .mcp-tool-call-demo {
      padding: 20px;
      max-width: 800px;
      margin: 0 auto;
    }

    .tool-call-form {
      margin-bottom: 20px;

      .form-group {
        margin-bottom: 15px;

        label {
          display: block;
          margin-bottom: 5px;
          font-weight: 600;
        }

        input, textarea {
          width: 100%;
          padding: 8px;
          border: 1px solid #ddd;
          border-radius: 4px;
          font-family: inherit;
        }

        textarea {
          font-family: 'Courier New', monospace;
          resize: vertical;
        }
      }
    }

    .call-button {
      background-color: #007bff;
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 4px;
      cursor: pointer;
      font-size: 16px;

      &:disabled {
        background-color: #6c757d;
        cursor: not-allowed;
      }

      &:hover:not(:disabled) {
        background-color: #0056b3;
      }
    }

    .error-message {
      background-color: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
      border-radius: 4px;
      padding: 15px;
      margin-bottom: 20px;
    }

    .response-display {
      border: 1px solid #ddd;
      border-radius: 4px;
      padding: 15px;

      .error-response {
        background-color: #f8d7da;
        color: #721c24;
        padding: 10px;
        border-radius: 4px;
        margin-bottom: 15px;
      }

      .content-block {
        margin-bottom: 15px;
        padding: 10px;
        border: 1px solid #eee;
        border-radius: 4px;

        h4 {
          margin-top: 0;
          margin-bottom: 10px;
          color: #495057;
        }

        pre {
          background-color: #f8f9fa;
          padding: 10px;
          border-radius: 4px;
          overflow-x: auto;
          margin: 0;
        }

        a {
          color: #007bff;
          text-decoration: none;

          &:hover {
            text-decoration: underline;
          }
        }
      }

      .structured-content {
        margin-top: 15px;
        padding-top: 15px;
        border-top: 1px solid #eee;
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class McpToolCallDemoComponent {
  protected serverName = 'example-server';
  protected toolName = 'example-tool';
  protected argumentsJson = '{"message": "Hello, world!"}';

  constructor(protected readonly toolCallService: McpToolCallService) { }

  async callTool() {
    let parsedArguments: Record<string, unknown> = {};

    try {
      if (this.argumentsJson.trim()) {
        parsedArguments = JSON.parse(this.argumentsJson) as Record<string, unknown>;
      }
    } catch (error) {
      console.error('Invalid JSON in arguments:', error);
      return;
    }

    await this.toolCallService.callTool(
      this.serverName,
      this.toolName,
      parsedArguments
    );
  }

  // Type-safe content extractors
  getTextContent(block: McpClientToolContentBlockView): string {
    return block.type === 'text' ? (block as any).text : '';
  }

  getImageData(block: McpClientToolContentBlockView): string {
    return block.type === 'image' ? (block as any).data : '';
  }

  getImageMediaType(block: McpClientToolContentBlockView): string {
    return block.type === 'image' ? (block as any).mediaType : '';
  }

  getAudioData(block: McpClientToolContentBlockView): string {
    return block.type === 'audio' ? (block as any).data : '';
  }

  getAudioMediaType(block: McpClientToolContentBlockView): string {
    return block.type === 'audio' ? (block as any).mediaType : '';
  }

  // Resource content extractors
  getResourceType(block: McpClientToolContentBlockView): string {
    return block.type === 'resource' ? (block as any).resource?.type : '';
  }

  getResourceUri(block: McpClientToolContentBlockView): string {
    return block.type === 'resource' ? (block as any).resource?.uri : '';
  }

  getResourceMediaType(block: McpClientToolContentBlockView): string | null {
    return block.type === 'resource' ? (block as any).resource?.mediaType : null;
  }

  getResourceTextContent(block: McpClientToolContentBlockView): string {
    const resource = block.type === 'resource' ? (block as any).resource : null;
    return resource?.type === 'text' ? resource.text : '';
  }

  getResourceBlobContent(block: McpClientToolContentBlockView): string {
    const resource = block.type === 'resource' ? (block as any).resource : null;
    return resource?.type === 'blob' ? resource.blob : '';
  }

  // Resource link extractors
  getResourceLinkUri(block: McpClientToolContentBlockView): string {
    return block.type === 'resource_link' ? (block as any).uri : '';
  }

  getResourceLinkName(block: McpClientToolContentBlockView): string {
    return block.type === 'resource_link' ? (block as any).name : '';
  }

  getResourceLinkDescription(block: McpClientToolContentBlockView): string | null {
    return block.type === 'resource_link' ? (block as any).description : null;
  }

  getResourceLinkMediaType(block: McpClientToolContentBlockView): string | null {
    return block.type === 'resource_link' ? (block as any).mediaType : null;
  }

  getResourceLinkSize(block: McpClientToolContentBlockView): number | null {
    return block.type === 'resource_link' ? (block as any).size : null;
  }
}
