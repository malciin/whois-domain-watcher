import { DomainInfo } from "./interfaces/domain-info";

const prefix = window.location.toString();

export async function getWhoisResponse(domain: string): Promise<string> {
  const response = await fetch(`${prefix}${domain}`);

  return await response.text();
}

export async function fetchWatchedDomains(): Promise<DomainInfo[]> {
  const response = await fetch(prefix, {
    headers: {
      "Accept": "application/json"
    }
  });
  const results = await response.json();
  
  return results as DomainInfo[];
}

export interface WatchResponse {
  ok: boolean;
  text: string;
  reason?: string;
}

export async function watchDomain(domain: string): Promise<WatchResponse> {
  const httpResponse = await fetch(prefix, {
    method: 'POST',
    headers: {
      "Content-Type": "plain/text"
    },
    body: domain
  });
  const watchResponse: WatchResponse = {
    ok: httpResponse.ok,
    text: await httpResponse.text()
  };

  if (!httpResponse.ok) {
    return { ...watchResponse, reason: watchResponse.text };
  }

  if (watchResponse.text.includes('already watched')) {
    return {
      ...watchResponse,
      ok: false,
      reason: `Domain ${domain} already watched!`
    };
  }

  return watchResponse;
}

export async function unwatchDomain(domain: string): Promise<void> {
  await fetch(prefix, {
    method: 'DELETE',
    headers: {
      "Content-Type": "plain/text"
    },
    body: domain
  });
}
