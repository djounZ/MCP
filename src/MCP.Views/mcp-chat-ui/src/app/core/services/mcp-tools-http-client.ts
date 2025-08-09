import { Injectable } from '@angular/core';
import { McpToolDescription } from '../../shared/models/mcp-tools-api.models';

@Injectable({
  providedIn: 'root'
})
export class McpToolsHttpClient {
  private readonly apiBaseUrl = 'http://localhost:5200';
  private readonly endpoints = {
    descriptions: '/api/mcp_tools/descriptions'
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
}
