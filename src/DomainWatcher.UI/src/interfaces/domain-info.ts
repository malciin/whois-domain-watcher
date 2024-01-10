import { QueryStatus } from "../enums/query-status";

export interface DomainInfo {
  domain: string;
  queryStatus: QueryStatus;
  queryTimestamp: string;
  expirationTimestamp?: string;
  registrationTimestamp?: string;
}
