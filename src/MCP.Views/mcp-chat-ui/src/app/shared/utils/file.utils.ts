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
  const blob = new Blob([json], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
