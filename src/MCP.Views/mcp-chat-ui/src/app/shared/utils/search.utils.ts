/**
 * Utility functions for implementing search functionality across different components
 */

/**
 * Configuration interface for search functionality
 */
export interface SearchConfig<T> {
  /**
   * Function to extract searchable text from an item
   */
  extractText: (item: T) => string[];
  /**
   * Optional function to extract tags from an item
   */
  extractTags?: (item: T) => string[];
  /**
   * Whether search should be case sensitive (default: false)
   */
  caseSensitive?: boolean;
  /**
   * Whether to match whole words only (default: false)
   */
  wholeWordOnly?: boolean;
}

/**
 * Performs a search filter on an array of items based on a query string
 * @param items - Array of items to search through
 * @param query - Search query string
 * @param config - Search configuration
 * @returns Filtered array of items that match the search criteria
 */
export function searchFilter<T>(
  items: T[],
  query: string,
  config: SearchConfig<T>
): T[] {
  const trimmedQuery = query.trim();

  if (!trimmedQuery) {
    return items;
  }

  const searchQuery = config.caseSensitive ? trimmedQuery : trimmedQuery.toLowerCase();

  return items.filter(item => {
    // Extract searchable text
    const textFields = config.extractText(item);
    const normalizedTextFields = config.caseSensitive
      ? textFields
      : textFields.map(text => text.toLowerCase());

    // Extract tags if extractor is provided
    const tags = config.extractTags ? config.extractTags(item) : [];
    const normalizedTags = config.caseSensitive
      ? tags
      : tags.map(tag => tag.toLowerCase());

    // Combine all searchable content
    const allSearchableContent = [...normalizedTextFields, ...normalizedTags];

    // Check if any field matches the query
    return allSearchableContent.some(content => {
      if (config.wholeWordOnly) {
        // Create regex for whole word matching
        const wordBoundaryRegex = new RegExp(`\\b${escapeRegExp(searchQuery)}\\b`, 'i');
        return wordBoundaryRegex.test(content);
      } else {
        return content.includes(searchQuery);
      }
    });
  });
}

/**
 * Highlights search matches in text with HTML markup
 * @param text - Text to highlight
 * @param query - Search query to highlight
 * @param highlightClass - CSS class to apply to highlighted text (default: 'search-highlight')
 * @param caseSensitive - Whether highlighting should be case sensitive (default: false)
 * @returns Text with highlighted matches
 */
export function highlightSearchMatches(
  text: string,
  query: string,
  highlightClass: string = 'search-highlight',
  caseSensitive: boolean = false
): string {
  const trimmedQuery = query.trim();

  if (!trimmedQuery || !text) {
    return text;
  }

  const flags = caseSensitive ? 'g' : 'gi';
  const escapedQuery = escapeRegExp(trimmedQuery);
  const regex = new RegExp(`(${escapedQuery})`, flags);

  return text.replace(regex, `<span class="${highlightClass}">$1</span>`);
}

/**
 * Escapes special regex characters in a string
 * @param string - String to escape
 * @returns Escaped string safe for use in regex
 */
function escapeRegExp(string: string): string {
  return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

/**
 * Extracts searchable text from common object properties
 * @param item - Object to extract text from
 * @param properties - Array of property names to extract
 * @returns Array of extracted text values
 */
export function extractTextFromProperties<T>(
  item: T,
  properties: (keyof T)[]
): string[] {
  return properties
    .map(prop => {
      const value = item[prop];
      if (typeof value === 'string') {
        return value;
      } else if (value && typeof value === 'object' && 'toString' in value) {
        return String(value);
      }
      return '';
    })
    .filter(text => text.length > 0);
}

/**
 * Search utility class for managing search state and operations
 */
export class SearchManager<T> {
  private _query: string = '';
  private _filteredItems: T[] = [];
  private _originalItems: T[] = [];

  constructor(private config: SearchConfig<T>) { }

  /**
   * Sets the items to search through
   */
  setItems(items: T[]): void {
    this._originalItems = [...items];
    this.applyFilter();
  }

  /**
   * Updates the search query and applies filtering
   */
  setQuery(query: string): void {
    this._query = query;
    this.applyFilter();
  }

  /**
   * Gets the current search query
   */
  get query(): string {
    return this._query;
  }

  /**
   * Gets the filtered items based on current query
   */
  get filteredItems(): T[] {
    return this._filteredItems;
  }

  /**
   * Gets the original unfiltered items
   */
  get originalItems(): T[] {
    return this._originalItems;
  }

  /**
   * Checks if search is currently active
   */
  get isSearchActive(): boolean {
    return this._query.trim().length > 0;
  }

  /**
   * Gets the count of filtered results
   */
  get resultCount(): number {
    return this._filteredItems.length;
  }

  /**
   * Clears the search query and shows all items
   */
  clearSearch(): void {
    this._query = '';
    this.applyFilter();
  }

  private applyFilter(): void {
    this._filteredItems = searchFilter(this._originalItems, this._query, this.config);
  }
}
