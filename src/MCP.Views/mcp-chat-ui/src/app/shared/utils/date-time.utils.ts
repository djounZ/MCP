/**
 * Date and time utility functions for formatting and display
 */

/**
 * Formats a date into a user-friendly time display
 * @param messageTime The date to format
 * @returns A formatted string like "Just now", "5m ago", "3:03 PM", etc.
 */
export function formatMessageTime(messageTime: Date): string {
  const now = new Date();
  const diffInMilliseconds = now.getTime() - messageTime.getTime();
  const diffInMinutes = Math.floor(diffInMilliseconds / (1000 * 60));
  const diffInHours = Math.floor(diffInMinutes / 60);
  const diffInDays = Math.floor(diffInHours / 24);

  // If message is from today
  if (diffInDays === 0) {
    // If less than 1 minute ago
    if (diffInMinutes < 1) {
      return 'Just now';
    }
    // If less than 1 hour ago
    else if (diffInHours < 1) {
      return `${diffInMinutes}m ago`;
    }
    // Same day, show time
    else {
      return messageTime.toLocaleTimeString('en-US', {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
      });
    }
  }
  // If message is from yesterday
  else if (diffInDays === 1) {
    return `Yesterday ${messageTime.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    })}`;
  }
  // If message is from this year
  else if (messageTime.getFullYear() === now.getFullYear()) {
    return messageTime.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  }
  // If message is from a previous year
  else {
    return messageTime.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  }
}
