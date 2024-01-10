import { Accessor, Component, createSignal } from "solid-js";
import { DomainInfo } from "../interfaces/domain-info";
import { QueryStatus } from "../enums/query-status";
import { jiraStyleDuration, secondsDifference } from "../utils/duration.utils";
import { Modal } from "./modal";
import { getWhoisResponse } from "../requests";

const durationFormatter = (seconds: number): string => jiraStyleDuration(seconds, { maxSegments: 2 })

function getExpiration(info: DomainInfo, now: Date): string {
  if (info.queryStatus === QueryStatus.ParserMissing) return 'Parser missing';
  if (info.queryStatus === QueryStatus.TakenButTimestampsHidden) return 'Hidden';
  if (!info.expirationTimestamp) return '-';
  return durationFormatter(secondsDifference(new Date(info.expirationTimestamp), now));
}

function getClassList(info: DomainInfo): string {
  const classes: string[] = [ 'domain' ];

  if (info.queryStatus === QueryStatus.ParserMissing) classes.push('parser-missing');
  else if (info.queryStatus === QueryStatus.TakenButTimestampsHidden) classes.push('taken-hidden-timestamps');
  else if (!info.expirationTimestamp && info.queryStatus === QueryStatus.OKParsedByFallback) classes.push('free callback-parser');
  else if (!info.expirationTimestamp) classes.push('free');

  return classes.join(' ');
}

interface DomainTableRowProps {
  entry: DomainInfo;
  nowDateAccessor: Accessor<Date>;
  unwatchCallback: (row: DomainInfo) => void;
}

export const DomainTableRow: Component<DomainTableRowProps> = props => {
  const [shownWhois, setShownWhois] = createSignal('');
  const nowDate = props.nowDateAccessor();
  const { entry } = props;



  return <>
    <tr class={getClassList(entry)}>
      <td>{ entry.domain }</td>
      <td>{ getExpiration(entry, nowDate) }</td>
      <td>{ durationFormatter(secondsDifference(nowDate, new Date(entry.queryTimestamp))) }</td>
      <td class="action-buttons">
        <button class="tiny" onClick={ _ => getWhoisResponse(entry.domain).then(x => setShownWhois(x)) }>WHOIS</button>
        <button class="tiny" onClick={ _ => props.unwatchCallback(entry) }>Unwatch</button>
      </td>
    </tr>

    <Modal
      show={!!shownWhois()}
      title={ 'Whois response for ' + entry.domain }
      close={() => setShownWhois('')}
      class="whois-response">
      { shownWhois() }
    </Modal>
  </>
};