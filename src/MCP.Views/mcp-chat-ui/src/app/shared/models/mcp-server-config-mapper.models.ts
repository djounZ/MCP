import { McpServerConfigurationItem } from './mcp-server-config-api.models';
import { McpServerConfigurationView } from './mcp-server-config-view.models';

export function toMcpServerConfigurationView(api: McpServerConfigurationItem): McpServerConfigurationView {
    return {
        category: api.category ?? null,
        command: api.command ?? null,
        args: api.args ?? null,
        env: api.env ?? null,
        url: api.url ?? null,
        type: api.type ?? null
    };
}

export function fromMcpServerConfigurationView(view: McpServerConfigurationView): McpServerConfigurationItem {
    return {
        category: view.category ?? null,
        command: view.command ?? null,
        args: view.args ?? null,
        env: view.env ?? null,
        url: view.url ?? null,
        type: view.type ?? null
    };
}
