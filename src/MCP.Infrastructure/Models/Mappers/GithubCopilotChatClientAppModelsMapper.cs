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

    #region Reverse Mappings (AppModel to Microsoft.Extensions.AI)

    // Chat completion reverse mappings
    public ChatResponse MapFromAppModel(ChatResponseAppModel appModel)
    {
        var chatResponse = new ChatResponse([.. appModel.Messages.Select(MapFromAppModel)])
        {
            ResponseId = appModel.ResponseId,
            ConversationId = appModel.ConversationId,
            ModelId = appModel.ModelId,
            CreatedAt = appModel.CreatedAt,
            FinishReason = MapFromAppModelFinishReason(appModel.FinishReason)
        };
        return chatResponse;
    }

    public ChatResponseUpdate MapFromAppModel(ChatResponseUpdateAppModel appModel)
    {
        return new ChatResponseUpdate
        {
            AuthorName = appModel.AuthorName,
            Role = MapFromAppModelRole(appModel.Role ?? ChatRoleEnumAppModel.User),
            Contents = [.. appModel.Contents.Select(MapFromAppModelContent)],
            ResponseId = appModel.ResponseId,
            MessageId = appModel.MessageId,
            ConversationId = appModel.ConversationId,
            CreatedAt = appModel.CreatedAt,
            FinishReason = MapFromAppModelFinishReason(appModel.FinishReason),
            ModelId = appModel.ModelId
        };
    }

    public ChatMessage MapFromAppModel(ChatMessageAppModel appModel)
    {
        var contents = appModel.Contents.Select(MapFromAppModelContent).ToArray();
        return new ChatMessage(MapFromAppModelRole(appModel.Role), contents);
    }

    public ChatOptions? MapFromAppModel(ChatOptionsAppModel? appModel)
    {
        if (appModel == null)
        {
            return null;
        }

        return new ChatOptions
        {
            ConversationId = appModel.ConversationId,
            Instructions = appModel.Instructions,
            Temperature = appModel.Temperature,
            MaxOutputTokens = appModel.MaxOutputTokens,
            TopP = appModel.TopP,
            TopK = appModel.TopK,
            FrequencyPenalty = appModel.FrequencyPenalty,
            PresencePenalty = appModel.PresencePenalty,
            Seed = appModel.Seed,
            ResponseFormat = MapFromAppModel(appModel.ResponseFormat),
            ModelId = appModel.ModelId,
            StopSequences = appModel.StopSequences,
            AllowMultipleToolCalls = appModel.AllowMultipleToolCalls,
            ToolMode = MapFromAppModel(appModel.ToolMode)
        };
    }

    private ChatToolMode? MapFromAppModel(ChatToolModeAppModel? appModel)
    {
        return appModel switch
        {
            AutoChatToolModeAppModel => new AutoChatToolMode(),
            NoneChatToolModeAppModel => new NoneChatToolMode(),
            RequiredChatToolModeAppModel required => new RequiredChatToolMode(required.RequiredFunctionName),
            _ => null
        };
    }

    private ChatResponseFormat? MapFromAppModel(ChatResponseFormatAppModel? appModel)
    {
        return appModel switch
        {
            ChatResponseFormatTextAppModel => new ChatResponseFormatText(),
            ChatResponseFormatJsonAppModel json => new ChatResponseFormatJson(
                json.Schema,
                json.SchemaName,
                json.SchemaDescription
            ),
            _ => null
        };
    }

    // Role enum reverse mapping
    public ChatRole MapFromAppModel(ChatRoleEnumAppModel appModel)
    {
        return MapFromAppModelRole(appModel);
    }

    // Content reverse mappings
    public TextContent MapFromAppModel(TextContentAppModel appModel)
    {
        return new TextContent(appModel.Text);
    }

    public DataContent MapFromAppModel(DataContentAppModel appModel)
    {
        return new DataContent(appModel.Uri.ToString(), appModel.MediaType);
    }

    public UsageContent MapFromAppModel(UsageContentAppModel appModel)
    {
        return new UsageContent(MapFromAppModel(appModel.DetailsAppModel));
    }

    public UriContent MapFromAppModel(UriContentAppModel appModel)
    {
        return new UriContent(appModel.Uri, appModel.MediaType);
    }

    public ErrorContent MapFromAppModel(ErrorContentAppModel appModel)
    {
        return new ErrorContent(appModel.Message);
    }

    // Usage reverse mappings
    public UsageDetails MapFromAppModel(UsageDetailsAppModel appModel)
    {
        var additionalCounts = appModel.AdditionalCounts != null
            ? new AdditionalPropertiesDictionary<long>(appModel.AdditionalCounts)
            : null;

        return new UsageDetails
        {
            InputTokenCount = appModel.InputTokenCount,
            OutputTokenCount = appModel.OutputTokenCount,
            TotalTokenCount = appModel.TotalTokenCount,
            AdditionalCounts = additionalCounts
        };
    }

    // Helper methods for reverse mapping specific types
    private ChatRole MapFromAppModelRole(ChatRoleEnumAppModel appModel)
    {
        return appModel switch
        {
            ChatRoleEnumAppModel.System => ChatRole.System,
            ChatRoleEnumAppModel.User => ChatRole.User,
            ChatRoleEnumAppModel.Assistant => ChatRole.Assistant,
            ChatRoleEnumAppModel.Tool => ChatRole.Tool,
            _ => ChatRole.User
        };
    }

    private AIChatFinishReason? MapFromAppModelFinishReason(ChatFinishReasonAppModel? appModel)
    {
        return appModel switch
        {
            ChatFinishReasonAppModel.Stop => AIChatFinishReason.Stop,
            ChatFinishReasonAppModel.Length => AIChatFinishReason.Length,
            ChatFinishReasonAppModel.ToolCalls => AIChatFinishReason.ToolCalls,
            ChatFinishReasonAppModel.ContentFilter => AIChatFinishReason.ContentFilter,
            _ => null
        };
    }

    private AIContent MapFromAppModelContent(AiContentAppModel appModel)
    {
        return appModel switch
        {
            TextContentAppModel text => MapFromAppModel(text),
            DataContentAppModel data => MapFromAppModel(data),
            UsageContentAppModel usage => MapFromAppModel(usage),
            UriContentAppModel uri => MapFromAppModel(uri),
            ErrorContentAppModel error => MapFromAppModel(error),
            _ => throw new NotSupportedException($"Unsupported app model content type: {appModel.GetType().Name}")
        };
    }

    #endregion
}

