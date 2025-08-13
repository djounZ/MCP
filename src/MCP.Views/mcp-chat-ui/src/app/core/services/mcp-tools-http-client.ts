import { Injectable } from '@angular/core';
import { McpToolDescription, McpClientToolRequest, McpClientToolResponse } from '../../shared/models/mcp-tools-api.models';

@Injectable({
  providedIn: 'root'
})
export class McpToolsHttpClient {
  private readonly apiBaseUrl = 'http://localhost:5200';
  private readonly endpoints = {
    descriptions: '/api/mcp_tools/descriptions',
    call: '/api/mcp_tools/call'
  };

  async getToolDescriptions(): Promise<Map<string, McpToolDescription[]>> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.descriptions, {
        method: 'GET'
      });
      if (!response.ok) {
        throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      }
      const data = await response.json() as { [key: string]: McpToolDescription[] };
      return new Map(Object.entries(data));
    } catch (error) {
      console.error('Failed to fetch MCP tool descriptions:', error);
      return new Map();
    }
  }

  async callTool(request: McpClientToolRequest): Promise<McpClientToolResponse> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.call, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      }

      const data = await response.json() as McpClientToolResponse;
      return data;
    } catch (error) {
      console.error('Failed to call MCP tool:', error);
      // Return a default error response
      return {
        content: [{
          $type: 'text',
          text: `Error calling tool: ${error instanceof Error ? error.message : 'Unknown error'}`,
          _meta: null,
          annotations: null
        }],
        structured_content: null,
        isError: true
      };
    }
  }
}
