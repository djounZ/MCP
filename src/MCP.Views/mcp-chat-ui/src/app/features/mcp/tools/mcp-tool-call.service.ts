import { Injectable, signal } from '@angular/core';
import { McpToolsHttpClient } from '../../../core/services/mcp-tools-http-client';
import {
  McpClientToolRequestView,
  McpClientToolResponseView
} from '../../../shared/models/mcp-tools-view.models';
import {
  fromMcpClientToolRequestView,
  toMcpClientToolResponseView
} from '../../../shared/models/mcp-tools-mapper.models';

@Injectable({
  providedIn: 'root'
})
export class McpToolCallService {
  private readonly httpClient = new McpToolsHttpClient();

  // Signal to track the current tool call status
  protected readonly isLoading = signal(false);
  protected readonly lastResponse = signal<McpClientToolResponseView | null>(null);
  protected readonly lastError = signal<string | null>(null);

  constructor() { }

  /**
   * Call an MCP tool with the provided parameters
   * @param serverName The name of the MCP server
   * @param toolName The name of the tool to call
   * @param toolArguments The arguments to pass to the tool
   * @returns Promise containing the tool response
   */
  async callTool(
    serverName: string,
    toolName: string,
    toolArguments: Record<string, unknown> = {}
  ): Promise<McpClientToolResponseView | null> {
    this.isLoading.set(true);
    this.lastError.set(null);

    try {
      const requestView: McpClientToolRequestView = {
        serverName,
        toolName,
        arguments: toolArguments
      };

      // Convert to API model and make the call
      const apiRequest = fromMcpClientToolRequestView(requestView);
      const apiResponse = await this.httpClient.callTool(apiRequest);

      // Convert back to view model
      const responseView = toMcpClientToolResponseView(apiResponse);

      this.lastResponse.set(responseView);
      return responseView;

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error occurred';
      this.lastError.set(errorMessage);
      console.error('Failed to call MCP tool:', error);
      return null;
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Get the current loading state
   */
  getIsLoading() {
    return this.isLoading.asReadonly();
  }

  /**
   * Get the last response
   */
  getLastResponse() {
    return this.lastResponse.asReadonly();
  }

  /**
   * Get the last error
   */
  getLastError() {
    return this.lastError.asReadonly();
  }

  /**
   * Clear the last response and error
   */
  clear() {
    this.lastResponse.set(null);
    this.lastError.set(null);
  }
}
