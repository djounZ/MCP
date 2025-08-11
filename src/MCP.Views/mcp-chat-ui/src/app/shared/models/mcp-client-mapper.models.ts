import { McpClientDescription, McpClientDescriptionDictionary } from './mcp-client-api.models';
import { McpClientDescriptionView, McpClientDescriptionDictionaryView } from './mcp-client-view.models';

export function toMcpClientDescriptionView(api: McpClientDescription): McpClientDescriptionView {
    return {
        tools: api.tools.map(tool => ({
            name: tool.name,
            description: tool.description ?? null,
            inputSchema: tool.input_schema ?? null,
            outputSchema: tool.output_schema ?? null
        })),
        prompts: api.prompts.map(prompt => ({
            name: prompt.name,
            title: prompt.title ?? null,
            description: prompt.description ?? null,
            arguments: prompt.arguments?.map(arg => ({
                name: arg.name,
                title: arg.title ?? null,
                description: arg.description ?? null,
                required: arg.required ?? null
            })) ?? null,
            meta: prompt._meta ?? null
        })),
        resourceTemplates: api.resource_templates.map(tpl => ({
            name: tpl.name,
            title: tpl.title ?? null,
            uriTemplate: tpl.uriTemplate ?? null,
            description: tpl.description ?? null,
            mimeType: tpl.mimeType ?? null,
            annotations: tpl.annotations ?? null,
            meta: tpl._meta ?? null
        })),
        resources: api.resources.map(res => ({
            name: res.name,
            title: res.title ?? null,
            uri: res.uri ?? null,
            description: res.description ?? null,
            mimeType: res.mimeType ?? null,
            annotations: res.annotations ?? null,
            size: res.size ?? null,
            meta: res._meta ?? null
        }))
    };
}

export function fromMcpClientDescriptionView(view: McpClientDescriptionView): McpClientDescription {
    return {
        tools: view.tools.map(tool => ({
            name: tool.name,
            description: tool.description ?? null,
            input_schema: tool.inputSchema ?? null,
            output_schema: tool.outputSchema ?? null
        })),
        prompts: view.prompts.map(prompt => ({
            name: prompt.name,
            title: prompt.title ?? null,
            description: prompt.description ?? null,
            arguments: prompt.arguments?.map(arg => ({
                name: arg.name,
                title: arg.title ?? null,
                description: arg.description ?? null,
                required: arg.required ?? null
            })) ?? null,
            _meta: prompt.meta ?? null
        })),
        resource_templates: view.resourceTemplates.map(tpl => ({
            name: tpl.name,
            title: tpl.title ?? null,
            uriTemplate: tpl.uriTemplate ?? null,
            description: tpl.description ?? null,
            mimeType: tpl.mimeType ?? null,
            annotations: tpl.annotations ?? null,
            _meta: tpl.meta ?? null
        })),
        resources: view.resources.map(res => ({
            name: res.name,
            title: res.title ?? null,
            uri: res.uri ?? null,
            description: res.description ?? null,
            mimeType: res.mimeType ?? null,
            annotations: res.annotations ?? null,
            size: res.size ?? null,
            _meta: res.meta ?? null
        }))
    };
}

export function toMcpClientDescriptionDictionaryView(apiDict: McpClientDescriptionDictionary): McpClientDescriptionDictionaryView {
    const result: McpClientDescriptionDictionaryView = {};
    for (const key in apiDict) {
        result[key] = toMcpClientDescriptionView(apiDict[key]);
    }
    return result;
}

export function fromMcpClientDescriptionDictionaryView(viewDict: McpClientDescriptionDictionaryView): McpClientDescriptionDictionary {
    const result: McpClientDescriptionDictionary = {};
    for (const key in viewDict) {
        result[key] = fromMcpClientDescriptionView(viewDict[key]);
    }
    return result;
}
