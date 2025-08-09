import { ChangeDetectionStrategy, Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MarkdownComponent } from 'ngx-markdown';
import { McpToolsHttpClient } from '../../../../core/services/mcp-tools-http-client';
import { McpToolDescriptionView, McpToolGroupView } from '../../../../shared/models/mcp-tools-view.models';
import { toMcpToolDescriptionView } from '../../../../shared/models/mcp-tools-mapper.models';
import { SearchConfig, searchFilter } from '../../../../shared/utils/search.utils';

@Component({
  selector: 'app-mcp-tools-description',
  standalone: true,
  imports: [CommonModule, FormsModule, MarkdownComponent],
  templateUrl: './description.html',
  styleUrls: ['./description.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class McpToolsDescriptionComponent {
  private allToolGroups: McpToolGroupView[] = []; // Store original data
  protected readonly searchQuery = signal('');
  protected readonly isLoading = signal(true);
  protected expandedGroups = new Set<string>(); // Track which groups are expanded
  private readonly mcpToolsHttpClient = inject(McpToolsHttpClient);

  // Search configuration for tools
  private readonly toolSearchConfig: SearchConfig<McpToolDescriptionView> = {
    extractText: (tool) => [
      tool.name,
      tool.title || '',
      tool.description || '',
      tool.author || ''
    ].filter(text => text.length > 0),
    extractTags: (tool) => tool.tags || [],
    caseSensitive: false,
    wholeWordOnly: false
  };

  // Computed properties for filtering
  protected readonly toolGroups = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    if (!query) {
      return this.allToolGroups;
    }

    return this.allToolGroups.map(group => ({
      ...group,
      tools: searchFilter(group.tools, query, this.toolSearchConfig)
    })).filter(group => group.tools.length > 0);
  }); protected readonly totalToolsCount = computed(() =>
    this.toolGroups().reduce((total, group) => total + group.tools.length, 0)
  );

  protected readonly filteredGroupsCount = computed(() => this.toolGroups().length);
  protected readonly originalGroupsCount = computed(() => this.allToolGroups.length);

  constructor() {
    this.loadTools();
  }

  async loadTools() {
    this.isLoading.set(true);
    const apiToolsMap = await this.mcpToolsHttpClient.getToolDescriptions();
    if (apiToolsMap instanceof Map) {
      this.allToolGroups = Array.from(apiToolsMap.entries()).map(([groupName, tools]) => ({
        groupName,
        tools: tools.map(toMcpToolDescriptionView)
      }));

      // All groups start collapsed by default
      this.expandedGroups.clear();
    } else {
      this.allToolGroups = [];
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

  protected updateSearchQuery(query: string): void {
    this.searchQuery.set(query);
    // Auto-expand groups when searching
    if (query.trim()) {
      this.toolGroups().forEach(group => {
        this.expandedGroups.add(group.groupName);
      });
    }
  }

  protected clearSearch(): void {
    this.searchQuery.set('');
    // Collapse all groups when clearing search
    this.expandedGroups.clear();
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
