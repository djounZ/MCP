using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

public record GithubCopilotAccessTokenResponse(
    [property: JsonPropertyName("annotations_enabled")] bool AnnotationsEnabled,
    [property: JsonPropertyName("blackbird_clientside_indexing")] bool BlackbirdClientsideIndexing,
    [property: JsonPropertyName("chat_enabled")] bool ChatEnabled,
    [property: JsonPropertyName("chat_jetbrains_enabled")] bool ChatJetbrainsEnabled,
    [property: JsonPropertyName("code_quote_enabled")] bool CodeQuoteEnabled,
    [property: JsonPropertyName("code_review_enabled")] bool CodeReviewEnabled,
    [property: JsonPropertyName("codesearch")] bool CodeSearch,
    [property: JsonPropertyName("copilotignore_enabled")] bool CopilotIgnoreEnabled,
    [property: JsonPropertyName("endpoints")] Endpoints Endpoints,
    [property: JsonPropertyName("enterprise_list")] int[] EnterpriseList,
    [property: JsonPropertyName("expires_at")] long ExpiresAt,
    [property: JsonPropertyName("individual")] bool Individual,
    [property: JsonPropertyName("limited_user_quotas")] int? LimitedUserQuotas,
    [property: JsonPropertyName("limited_user_reset_date")] long? LimitedUserResetDate,
    [property: JsonPropertyName("organization_list")] string[] OrganizationList,
    [property: JsonPropertyName("prompt_8k")] bool Prompt8K,
    [property: JsonPropertyName("public_suggestions")] string PublicSuggestions,
    [property: JsonPropertyName("refresh_in")] int RefreshIn,
    [property: JsonPropertyName("sku")] string Sku,
    [property: JsonPropertyName("snippy_load_test_enabled")] bool SnippyLoadTestEnabled,
    [property: JsonPropertyName("telemetry")] string Telemetry,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("tracking_id")] string TrackingId,
    [property: JsonPropertyName("vsc_electron_fetcher_v2")] bool VscElectronFetcherV2,
    [property: JsonPropertyName("xcode")] bool Xcode,
    [property: JsonPropertyName("xcode_chat")] bool XcodeChat
);

public record Endpoints(
    [property: JsonPropertyName("api")] string Api,
    [property: JsonPropertyName("origin-tracker")] string OriginTracker,
    [property: JsonPropertyName("proxy")] string Proxy,
    [property: JsonPropertyName("telemetry")] string Telemetry
);
