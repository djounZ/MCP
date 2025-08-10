export function formatFromSnakeCase(input: string): string {
  return input
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}

/**
 * Detects if text content contains markdown patterns
 * @param text The text to analyze
 * @returns true if markdown patterns are detected, false otherwise
 */
export function isMarkdownContent(text: string): boolean {
  if (!text) {
    return false;
  }

  // Check for common markdown patterns
  const markdownPatterns = [
    /^#{1,6}\s/m,          // Headers
    /\*\*.*\*\*/,          // Bold
    /\*.*\*/,              // Italic
    /`.*`/,                // Inline code
    /```[\s\S]*```/,       // Code blocks
    /^\* /m,               // Unordered lists
    /^\d+\. /m,            // Ordered lists
    /\[.*\]\(.*\)/,        // Links
    /^\> /m,               // Blockquotes
    /^\|.*\|/m,            // Tables
  ];

  return markdownPatterns.some(pattern => pattern.test(text));
}
