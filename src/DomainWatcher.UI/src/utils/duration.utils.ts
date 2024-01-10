/// performs a - b computation with probably negative result is b > a
export function secondsDifference(a: Date, b: Date): number {
  return Math.floor((a.getTime() - b.getTime()) / 1000);
}

export function clockFormatter(secondsOrNull: number | null, nullLabel: string = '-'): string {
  if (secondsOrNull === null) {
    return nullLabel;
  }
  
  const date = new Date(secondsOrNull * 1000);

  return date.toISOString().slice(11, 19);
}

export interface JiraStyleDurationOptions {
  nullLabel?: string;
  maxSegments?: number;
} 

export function jiraStyleDuration(secondsOrNull: number | null, options: JiraStyleDurationOptions | null = null): string {
  options ??= {}
  options.nullLabel ??= '-';

  if (secondsOrNull === null) {
    return options.nullLabel;
  }

  const segments = [];
  const dividers = [
    { divider: 60, suffix: 's' },
    { divider: 60, suffix: 'm' },
    { divider: 24, suffix: 'h' },
    { divider: 365, suffix: 'd' },
    { divider: 1, suffix: 'y' }
  ];
  let iteration = 86400 * 365;

  while (dividers.length > 0) {
    const { suffix, divider } = dividers.pop()!;
    iteration = iteration / divider;
    
    const value = Math.floor(secondsOrNull / iteration);

    if (value > 0) {
      segments.push(`${value}${suffix}`);
    }

    if (segments.length === options.maxSegments) break;

    secondsOrNull -= value * iteration;
  }

  return segments.join(' ');
}
