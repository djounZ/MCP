import { Injectable } from '@angular/core';
import { McpClientDescriptionDictionary } from '../../shared/models/mcp-client-api.models';

@Injectable({
  providedIn: 'root'
})
export class McpClientHttpClient {
  private readonly apiBaseUrl = 'http://localhost:5200';
  private readonly endpoints = {
    descriptions: '/mcp-client-descriptions'
  };

  async getClientDescriptions(): Promise<McpClientDescriptionDictionary> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.descriptions, {
        method: 'GET'
      });
      if (!response.ok) {
        throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      }
      const data = await response.json() as McpClientDescriptionDictionary;
      return data;
    } catch (error) {
      console.error('Failed to fetch MCP client descriptions:', error);
      return {};
    }
  }
}
