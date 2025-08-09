import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { McpToolsHttpClient } from '../../../../core/services/mcp-tools-http-client';
import { McpToolDescriptionView, McpToolGroupView } from '../../../../shared/models/mcp-tools-view.models';
import { toMcpToolDescriptionView } from '../../../../shared/models/mcp-tools-mapper.models';

@Component({
  selector: 'app-mcp-tools-description',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './description.html',
  styleUrls: ['./description.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class McpToolsDescriptionComponent {
  protected toolGroups: McpToolGroupView[] = [];
  protected totalToolsCount = 0;
  protected readonly isLoading = signal(true);
  protected expandedGroups = new Set<string>(); // Track which groups are expanded
  private readonly mcpToolsHttpClient = inject(McpToolsHttpClient);

  constructor() {
    this.loadTools();
  }

  async loadTools() {
    this.isLoading.set(true);
    const apiToolsMap = await this.mcpToolsHttpClient.getToolDescriptions();
    if (apiToolsMap instanceof Map) {
      this.toolGroups = Array.from(apiToolsMap.entries()).map(([groupName, tools]) => ({
        groupName,
        tools: tools.map(toMcpToolDescriptionView)
      }));
      this.totalToolsCount = this.toolGroups.reduce((total, group) => total + group.tools.length, 0);

      // All groups start collapsed by default
      this.expandedGroups.clear();
    } else {
      this.toolGroups = [];
      this.totalToolsCount = 0;
      console.error('MCP Tools API returned unexpected shape:', apiToolsMap);
    }
    this.isLoading.set(false);
  }

  protected toggleGroup(groupName: string): void {
    if (this.expandedGroups.has(groupName)) {
      this.expandedGroups.delete(groupName);
    } else {
      this.expandedGroups.add(groupName);
    }
  }

  protected isGroupExpanded(groupName: string): boolean {
    return this.expandedGroups.has(groupName);
  }

  protected formatSchema(schema: any): string {
    if (!schema) return '';
    try {
      return JSON.stringify(schema, null, 2);
    } catch (error) {
      return 'Invalid schema format';
    }
  }
}
