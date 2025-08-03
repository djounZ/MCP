// RFC4122 version 4 UUID generator using crypto.getRandomValues
export function generateUuid(): string {
  // RFC4122 version 4 UUID using crypto.getRandomValues
  return '10000000-1000-4000-8000-100000000000'.replace(/[018]/g, (c: string) =>
    (parseInt(c) ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> parseInt(c) / 4).toString(16)
  );
}
