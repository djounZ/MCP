using MCP.Application.DTOs.AI.ChatCompletion;
using MCP.Application.DTOs.AI.Contents;
using Microsoft.Extensions.AI;
using AIChatFinishReason = Microsoft.Extensions.AI.ChatFinishReason;

namespace MCP.Infrastructure.Models.Mappers;

public class GithubCopilotChatClientAppModelsMapper
{
    // Chat completion mappings
    public ChatResponseAppModel MapToAppModel(ChatResponse chatResponse)
    {
        return new ChatResponseAppModel(
            Messages: [.. chatResponse.Messages.Select(MapToAppModel)],
            ResponseId: chatResponse.ResponseId,
            ConversationId: chatResponse.ConversationId,
            ModelId: chatResponse.ModelId,
            CreatedAt: chatResponse.CreatedAt,
            FinishReason: MapToAppModelFinishReason(chatResponse.FinishReason)
        );
    }

    public ChatResponseUpdateAppModel MapToAppModel(ChatResponseUpdate chatResponseUpdate)
    {
        return new ChatResponseUpdateAppModel(
            AuthorName: chatResponseUpdate.AuthorName,
            Role: MapToAppModelRole(chatResponseUpdate.Role),
            Contents: [.. chatResponseUpdate.Contents.Select(MapToAppModelContent)],
            ResponseId: chatResponseUpdate.ResponseId,
            MessageId: chatResponseUpdate.MessageId,
            ConversationId: chatResponseUpdate.ConversationId,
            CreatedAt: chatResponseUpdate.CreatedAt,
            FinishReason: MapToAppModelFinishReason(chatResponseUpdate.FinishReason),
            ModelId: chatResponseUpdate.ModelId
        );
    }

    public ChatMessageAppModel MapToAppModel(ChatMessage chatMessage)
    {

        return new ChatMessageAppModel(
            Role: MapToAppModelRole(chatMessage.Role) ?? ChatRoleEnumAppModel.User,
            Contents: [.. chatMessage.Contents.Select(MapToAppModelContent)]
        );
    }

    public ChatOptionsAppModel? MapToAppModel(ChatOptions? chatOptions)
    {
        if (chatOptions == null)
        {
            return null;
        }

        return new ChatOptionsAppModel(
            ConversationId: chatOptions.ConversationId,
            Instructions: chatOptions.Instructions,
            Temperature: chatOptions.Temperature,
            MaxOutputTokens: chatOptions.MaxOutputTokens,
            TopP: chatOptions.TopP,
            TopK: chatOptions.TopK,
            FrequencyPenalty: chatOptions.FrequencyPenalty,
            PresencePenalty: chatOptions.PresencePenalty,
            Seed: chatOptions.Seed,
            ResponseFormat: MapToAppModel(chatOptions.ResponseFormat),
            ModelId: chatOptions.ModelId,
            StopSequences: chatOptions.StopSequences?.ToList(),
            AllowMultipleToolCalls: chatOptions.AllowMultipleToolCalls,
            ToolMode: MapToAppModel(chatOptions.ToolMode)
        );
    }

    private ChatToolModeAppModel? MapToAppModel(ChatToolMode? toolMode)
    {
        if( toolMode == null)
        {
            return null;
        }

        if( toolMode is AutoChatToolMode)
        {
            return new AutoChatToolModeAppModel();
        }
        if (toolMode is NoneChatToolMode)
        {
            return new NoneChatToolModeAppModel();
        }

        if (toolMode is RequiredChatToolMode requiredToolMode)
        {
            return new RequiredChatToolModeAppModel(requiredToolMode.RequiredFunctionName);
        }

        return null;
    }

    private ChatResponseFormatAppModel? MapToAppModel(ChatResponseFormat? chatOptionsResponseFormat)
    {
        if(chatOptionsResponseFormat == null)
        {
            return null;
        }

        if (chatOptionsResponseFormat is ChatResponseFormatText)
        {
            return new ChatResponseFormatTextAppModel();
        }

        if (chatOptionsResponseFormat is ChatResponseFormatJson chatResponseFormatJson)
        {
            return new ChatResponseFormatJsonAppModel(chatResponseFormatJson.Schema, chatResponseFormatJson.SchemaName, chatResponseFormatJson.SchemaDescription);
        }
        return null;
    }

    // Role enum mapping
    public ChatRoleEnumAppModel? MapToAppModel(ChatRole chatRole)
    {
        return MapToAppModelRole(chatRole);
    }

    // Content mappings
    public TextContentAppModel MapToAppModel(TextContent textContent)
    {
        return new TextContentAppModel(
            Annotations: null, // Microsoft.Extensions.AI.TextContent doesn't have annotations
            Text: textContent.Text
        );
    }

    public DataContentAppModel MapToAppModel(DataContent dataContent)
    {
        return new DataContentAppModel(
            Annotations: null, // Microsoft.Extensions.AI.DataContent doesn't have annotations
            Uri:  new Uri(dataContent.Uri),
            MediaType: dataContent.MediaType,
            Name: null // Not available in Microsoft.Extensions.AI.DataContent
        );
    }

    public UsageContentAppModel MapToAppModel(UsageContent usageContent)
    {

        return new UsageContentAppModel(
            Annotations: null, // Microsoft.Extensions.AI.UsageContent doesn't have annotations
            DetailsAppModel: MapToAppModel(usageContent.Details)
        );
    }

    public UriContentAppModel MapToAppModel(UriContent uriContent)
    {

        return new UriContentAppModel(
            Annotations: null, // Microsoft.Extensions.AI.UriContent doesn't have annotations
            Uri: uriContent.Uri,
            MediaType: uriContent.MediaType
        );
    }

    public ErrorContentAppModel MapToAppModel(ErrorContent errorContent)
    {

        return new ErrorContentAppModel(
            Annotations: null, // Microsoft.Extensions.AI.ErrorContent doesn't have annotations
            Message: errorContent.Message
        );
    }

    // Usage mappings
    public UsageDetailsAppModel MapToAppModel(UsageDetails usageDetails)
    {
        return new UsageDetailsAppModel(
            InputTokenCount: usageDetails.InputTokenCount,
            OutputTokenCount: usageDetails.OutputTokenCount,
            TotalTokenCount: usageDetails.TotalTokenCount,
            AdditionalCounts: usageDetails.AdditionalCounts
        );
    }

    // Helper methods for mapping specific types
    private ChatRoleEnumAppModel? MapToAppModelRole(ChatRole? role)
    {
        if (role == null)
        {
            return null;
        }

        if (role == ChatRole.System)
        {
            return ChatRoleEnumAppModel.System;
        }
        if (role == ChatRole.User)
        {
            return ChatRoleEnumAppModel.User;
        }
        if (role == ChatRole.Assistant)
        {
            return ChatRoleEnumAppModel.Assistant;
        }
        if (role == ChatRole.Tool)
        {
            return ChatRoleEnumAppModel.Tool;
        }
        return ChatRoleEnumAppModel.User;
    }

    private ChatFinishReasonAppModel? MapToAppModelFinishReason(AIChatFinishReason? finishReason)
    {
        if (finishReason == null)
        {
            return null;
        }

        if (finishReason == AIChatFinishReason.Stop)
        {
            return ChatFinishReasonAppModel.Stop;
        }

        if (finishReason == AIChatFinishReason.Length)
        {
            return ChatFinishReasonAppModel.Length;
        }

        if (finishReason == AIChatFinishReason.ToolCalls)
        {
            return ChatFinishReasonAppModel.ToolCalls;
        }

        if (finishReason == AIChatFinishReason.ContentFilter)
        {
            return ChatFinishReasonAppModel.ContentFilter;
        }

        return ChatFinishReasonAppModel.Stop;
    }

    private AiContentAppModel MapToAppModelContent(AIContent content)
    {
        return content switch
        {
            TextContent textContent => MapToAppModel(textContent),
            DataContent dataContent => MapToAppModel(dataContent),
            UsageContent usageContent => MapToAppModel(usageContent),
            UriContent uriContent => MapToAppModel(uriContent),
            ErrorContent errorContent => MapToAppModel(errorContent),
            _ =>
                throw new NotSupportedException($"Unsupported content type: {content.GetType().Name}")
        };

    }
}

