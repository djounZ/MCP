import { Injectable } from '@angular/core';
import { McpServerConfigurationItem } from '../../shared/models/mcp-server-config-api.models';
import { toMcpServerConfigurationView } from '../../shared/models/mcp-server-config-mapper.models';

@Injectable({
  providedIn: 'root'
})
export class McpServerConfigHttpClient {
  private readonly apiBaseUrl = 'http://localhost:5200';
  private readonly endpoints = {
    servers: '/mcp-servers',
    serverByName: (name: string) => `/mcp-servers/${encodeURIComponent(name)}`
  };

  async getAllServers(): Promise<Map<string, McpServerConfigurationItem>> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.servers, { method: 'GET' });
      if (!response.ok) throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      const data = await response.json();
      // If the backend returns a direct map, just parse it
      return new Map(Object.entries(data));
    } catch (error) {
      console.error('Failed to fetch all MCP servers:', error);
      return new Map();
    }
  }

  async getServerByName(serverName: string): Promise<McpServerConfigurationItem | null> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.serverByName(serverName), { method: 'GET' });
      if (!response.ok) throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      const data = await response.json() as McpServerConfigurationItem;
      // Optionally map to view model: toMcpServerConfigurationView(data)
      return data;
    } catch (error) {
      console.error(`Failed to fetch MCP server '${serverName}':`, error);
      return null;
    }
  }

  async createServer(serverName: string, item: McpServerConfigurationItem): Promise<McpServerConfigurationItem | null> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.serverByName(serverName), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(item)
      });
      if (!response.ok) throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      const data = await response.json() as McpServerConfigurationItem;
      // Optionally map to view model: toMcpServerConfigurationView(data)
      return data;
    } catch (error) {
      console.error(`Failed to create MCP server '${serverName}':`, error);
      return null;
    }
  }

  async updateServer(serverName: string, item: McpServerConfigurationItem): Promise<McpServerConfigurationItem | null> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.serverByName(serverName), {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(item)
      });
      if (!response.ok) throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      const data = await response.json() as McpServerConfigurationItem;
      // Optionally map to view model: toMcpServerConfigurationView(data)
      return data;
    } catch (error) {
      console.error(`Failed to update MCP server '${serverName}':`, error);
      return null;
    }
  }

  async deleteServer(serverName: string): Promise<boolean> {
    try {
      const response = await fetch(this.apiBaseUrl + this.endpoints.serverByName(serverName), { method: 'DELETE' });
      if (!response.ok) throw new Error(`HTTP error ${response.status}: ${response.statusText}`);
      return true;
    } catch (error) {
      console.error(`Failed to delete MCP server '${serverName}':`, error);
      return false;
    }
  }
}
