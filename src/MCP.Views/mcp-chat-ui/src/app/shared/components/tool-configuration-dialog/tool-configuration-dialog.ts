import { ChangeDetectionStrategy, Component, inject, signal, OnInit, computed } from '@angular/core';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTreeModule, MatTreeNestedDataSource } from '@angular/material/tree';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { NestedTreeControl } from '@angular/cdk/tree';
import { McpToolsHttpClient } from '../../../core/services/mcp-tools-http-client';
import { McpToolDescription } from '../../models/mcp-tools-api.models';
import { AiToolAppModelView } from '../../models/chat-completion-view.models';

interface ToolTreeNode {
    name: string;
    isServer?: boolean;
    isSelected: boolean;
    children?: ToolTreeNode[];
    description?: McpToolDescription;
}

@Component({
    selector: 'app-tool-configuration-dialog',
    imports: [
        MatDialogModule,
        MatButtonModule,
        MatTreeModule,
        MatIconModule,
        MatCheckboxModule
    ],
    templateUrl: './tool-configuration-dialog.html',
    styleUrl: './tool-configuration-dialog.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToolConfigurationDialog implements OnInit {
    private readonly mcpToolsClient = inject(McpToolsHttpClient);
    private readonly dialogRef = inject(MatDialogRef<ToolConfigurationDialog>);

    protected readonly isLoading = signal(false);
    protected readonly treeData = signal<ToolTreeNode[]>([]);

    protected readonly treeControl = new NestedTreeControl<ToolTreeNode>(node => node.children);
    protected readonly dataSource = new MatTreeNestedDataSource<ToolTreeNode>();

    protected readonly selectedTools = computed(() => {
        const tools = new Map<string, AiToolAppModelView[]>();
        this.collectSelectedTools(this.treeData(), tools);
        return tools;
    });

    ngOnInit(): void {
        this.loadTools();
    }

    private async loadTools(): Promise<void> {
        this.isLoading.set(true);
        try {
            const toolsMap = await this.mcpToolsClient.getToolDescriptions();
            const treeNodes: ToolTreeNode[] = [];

            for (const [serverName, tools] of toolsMap.entries()) {
                const serverNode: ToolTreeNode = {
                    name: serverName,
                    isServer: true,
                    isSelected: false,
                    children: tools.map(tool => ({
                        name: tool.name,
                        isSelected: false,
                        description: tool
                    }))
                };
                treeNodes.push(serverNode);
            }

            this.treeData.set(treeNodes);
            this.dataSource.data = treeNodes;
            this.treeControl.dataNodes = treeNodes;
        } catch (error) {
            console.error('Failed to load tools:', error);
        } finally {
            this.isLoading.set(false);
        }
    }

    private collectSelectedTools(nodes: ToolTreeNode[], tools: Map<string, AiToolAppModelView[]>): void {
        for (const node of nodes) {
            if (node.isServer && node.children) {
                const selectedChildren = node.children.filter(child => child.isSelected);
                if (selectedChildren.length > 0) {
                    tools.set(node.name, selectedChildren.map(child => ({ name: child.name })));
                }
            }
            if (node.children) {
                this.collectSelectedTools(node.children, tools);
            }
        }
    }

    hasChild = (_: number, node: ToolTreeNode) => !!node.children && node.children.length > 0;

    onNodeToggle(node: ToolTreeNode): void {
        node.isSelected = !node.isSelected;

        // If it's a server node, update all children
        if (node.isServer && node.children) {
            node.children.forEach(child => child.isSelected = node.isSelected);
        }

        // If it's a tool node, update parent if needed
        if (!node.isServer) {
            this.updateParentSelection(node);
        }

        // Trigger change detection by updating the signal
        this.treeData.set([...this.treeData()]);
    }

    private updateParentSelection(childNode: ToolTreeNode): void {
        // Find parent server node
        for (const serverNode of this.treeData()) {
            if (serverNode.children?.includes(childNode)) {
                const selectedChildren = serverNode.children.filter(child => child.isSelected);
                serverNode.isSelected = selectedChildren.length === serverNode.children.length;
                break;
            }
        }
    }

    isIndeterminate(node: ToolTreeNode): boolean {
        if (!node.isServer || !node.children) return false;
        const selectedChildren = node.children.filter(child => child.isSelected);
        return selectedChildren.length > 0 && selectedChildren.length < node.children.length;
    }

    onSave(): void {
        this.dialogRef.close(this.selectedTools());
    }

    onCancel(): void {
        this.dialogRef.close(null);
    }
}
