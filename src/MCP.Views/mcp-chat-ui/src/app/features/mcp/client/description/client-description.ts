import { ChangeDetectionStrategy, Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { McpClientHttpClient } from '../../../../core/services/mcp-client-http-client';
import { McpClientDescriptionView, McpClientDescriptionDictionaryView } from '../../../../shared/models/mcp-client-view.models';
import { toMcpClientDescriptionDictionaryView } from '../../../../shared/models/mcp-client-mapper.models';
import { SearchConfig, searchFilter } from '../../../../shared/utils/search.utils';

interface McpClientGroupView {
  serverName: string;
  description: McpClientDescriptionView;
}

@Component({
  selector: 'app-mcp-client-description',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './client-description.html',
  styleUrls: ['./client-description.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class McpClientDescriptionComponent {
  private allClientGroups: McpClientGroupView[] = []; // Store original data
  protected readonly searchQuery = signal('');
  protected readonly isLoading = signal(true);
  protected expandedGroups = new Set<string>(); // Track which groups are expanded
  protected expandedSections = new Map<string, Set<string>>(); // Track which sections are expanded per server
  private readonly mcpClientHttpClient = inject(McpClientHttpClient);

  // Search configuration for client descriptions
  private readonly clientSearchConfig: SearchConfig<McpClientGroupView> = {
    extractText: (client) => [
      client.serverName,
      ...client.description.tools.map(tool => tool.name),
      ...client.description.tools.map(tool => tool.description || ''),
      ...client.description.prompts.map(prompt => prompt.name),
      ...client.description.prompts.map(prompt => prompt.description || ''),
      ...client.description.resources.map(resource => resource.name),
      ...client.description.resources.map(resource => resource.description || ''),
      ...client.description.resourceTemplates.map(template => template.name),
      ...client.description.resourceTemplates.map(template => template.description || '')
    ].filter(text => text.length > 0),
    extractTags: (client) => [],
    caseSensitive: false,
    wholeWordOnly: false
  };

  // Computed properties for filtering
  protected readonly clientGroups = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    if (!query) {
      return this.allClientGroups;
    }

    return searchFilter(this.allClientGroups, query, this.clientSearchConfig);
  });

  protected readonly totalClientsCount = computed(() => this.clientGroups().length);
  protected readonly filteredGroupsCount = computed(() => this.clientGroups().length);
  protected readonly originalGroupsCount = computed(() => this.allClientGroups.length);

  constructor() {
    this.loadClientDescriptions();
  }

  async loadClientDescriptions() {
    this.isLoading.set(true);
    const apiClientDict = await this.mcpClientHttpClient.getClientDescriptions();
    const viewClientDict = toMcpClientDescriptionDictionaryView(apiClientDict);

    this.allClientGroups = Object.entries(viewClientDict).map(([serverName, description]) => ({
      serverName,
      description
    }));

    // All groups start collapsed by default
    this.expandedGroups.clear();
    this.expandedSections.clear();
    this.isLoading.set(false);
  }

  protected toggleGroup(serverName: string): void {
    if (this.expandedGroups.has(serverName)) {
      this.expandedGroups.delete(serverName);
    } else {
      this.expandedGroups.add(serverName);
    }
  }

  protected isGroupExpanded(serverName: string): boolean {
    return this.expandedGroups.has(serverName);
  }

  protected updateSearchQuery(query: string): void {
    this.searchQuery.set(query);
    // Auto-expand groups and sections when searching
    if (query.trim()) {
      this.clientGroups().forEach(group => {
        this.expandedGroups.add(group.serverName);

        // Auto-expand relevant sections
        if (!this.expandedSections.has(group.serverName)) {
          this.expandedSections.set(group.serverName, new Set<string>());
        }
        const serverSections = this.expandedSections.get(group.serverName)!;

        // Expand sections that have content
        if (group.description.tools.length > 0) serverSections.add('tools');
        if (group.description.prompts.length > 0) serverSections.add('prompts');
        if (group.description.resources.length > 0) serverSections.add('resources');
        if (group.description.resourceTemplates.length > 0) serverSections.add('resourceTemplates');
      });
    }
  }

  protected clearSearch(): void {
    this.searchQuery.set('');
    // Collapse all groups when clearing search
    this.expandedGroups.clear();
    this.expandedSections.clear();
  }

  protected toggleSection(serverName: string, sectionType: string): void {
    if (!this.expandedSections.has(serverName)) {
      this.expandedSections.set(serverName, new Set<string>());
    }
    const serverSections = this.expandedSections.get(serverName)!;

    if (serverSections.has(sectionType)) {
      serverSections.delete(sectionType);
    } else {
      serverSections.add(sectionType);
    }
  }

  protected isSectionExpanded(serverName: string, sectionType: string): boolean {
    const serverSections = this.expandedSections.get(serverName);
    return serverSections ? serverSections.has(sectionType) : false;
  }

  protected formatSchema(schema: any): string {
    if (!schema) return '';
    try {
      return JSON.stringify(schema, null, 2);
    } catch (error) {
      return 'Invalid schema format';
    }
  }

  protected getTotalToolsCount(description: McpClientDescriptionView): number {
    return description.tools.length;
  }

  protected getTotalPromptsCount(description: McpClientDescriptionView): number {
    return description.prompts.length;
  }

  protected getTotalResourcesCount(description: McpClientDescriptionView): number {
    return description.resources.length + description.resourceTemplates.length;
  }
}
