/**
 * Fetches a list of files from a remote API endpoint, filtering by extension and file type.
 * @param apiUrl The API endpoint URL
 * @param fileExtension The file extension to filter by (e.g. '.md')
 * @param apiName The name of the API (for error messages)
 * @returns Promise resolving to an array of files with name and download_url
 */
export async function fetchFilesFromUrlUrlUnsafe(apiUrl: string, fileExtension: string, apiName: string): Promise<Array<{ name: string; download_url: string }>> {
  const response = await fetch(apiUrl);
  if (!response.ok) throw new Error('Failed to fetch files from ' + apiName + '.');
  const files = await response.json();
  return (Array.isArray(files)
    ? files.filter((f) => f.type === 'file' && f.name.endsWith(fileExtension))
    : []);
}
/**
 * Reads and parses an uploaded JSON file
 * @param file The uploaded File object
 * @returns A Promise resolving to the parsed object
 */
/**
 * Reads and parses an uploaded JSON file, converting specified fields to Date objects
 * @param file The uploaded File object
 * @param dateFields Optional array of field names to parse as Date
 * @returns A Promise resolving to the parsed object with Dates
 */
/**
 * Reads and parses an uploaded JSON file, automatically converting ISO date strings to Date objects
 * @param file The uploaded File object
 * @returns A Promise resolving to the parsed object with Dates
 */
export function readJsonFile<T = unknown>(file: File): Promise<T> {
  return new Promise<T>((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      try {
        const result = reader.result as string;
        let parsed = JSON.parse(result);
        parsed = reviveAllDates(parsed);
        resolve(parsed);
      } catch (err) {
        reject(err);
      }
    };
    reader.onerror = () => reject(reader.error);
    reader.readAsText(file);
  });
}

/**
 * Recursively converts specified fields to Date objects in an object
 */
function reviveAllDates(obj: any): any {
  if (Array.isArray(obj)) {
    return obj.map(item => reviveAllDates(item));
  }
  if (obj && typeof obj === 'object') {
    for (const key of Object.keys(obj)) {
      if (typeof obj[key] === 'string' && isIsoDateString(obj[key])) {
        obj[key] = new Date(obj[key]);
      } else if (typeof obj[key] === 'object') {
        obj[key] = reviveAllDates(obj[key]);
      }
    }
  }
  return obj;
}

function isIsoDateString(value: string): boolean {
  // Simple ISO 8601 date string check
  return typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/.test(value);
}
/**
 * Utility for exporting objects as downloadable JSON files
 */
export function exportAsJsonFile(data: unknown, filename: string): void {
  const json = JSON.stringify(data, null, 2);
  const dataType = 'application/json';
  exportAsFile(json, dataType, filename);
}

export function exportAsFile(json: string, dataType: string, fileName: string) {
  const blob = new Blob([json], { type: dataType });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  a.click();
  URL.revokeObjectURL(url);
}

