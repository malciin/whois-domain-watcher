import { Component, For, Show, createSignal, onCleanup } from "solid-js";
import { DomainInfo } from "../interfaces/domain-info";
import { DomainTableRow } from "./domain-table-row";
import { unwatchDomain, watchDomain } from "../requests";

function filterRows(rows: DomainInfo[], filterString: string): DomainInfo[] {
  if (!filterString) {
    return rows;
  }

  if (!filterString.includes('*') && !filterString.includes('+')) {
    return rows.filter(x => x.domain.includes(filterString));
  }

  filterString = '^' + filterString + '$';
  const regex = new RegExp(filterString.replaceAll(".", "\\.").replaceAll("+", ".").replaceAll("*", ".+"));

  return rows.filter(x => regex.test(x.domain));
}

interface DomainsTableProps {
  rows: DomainInfo[];
  triggerRowsRefetch: () => void;
}

export const DomainsTable: Component<DomainsTableProps> = (props) => {
  const [getError, setError] = createSignal('');
  const [getNewDomain, setNewDomain] = createSignal('');
  const [getFilter, setFilter] = createSignal('');
  const [nowDate, setNowDate] = createSignal(new Date());
  const dateTimerInterval = setInterval(() => setNowDate(new Date()), 1000);
  onCleanup(() => clearInterval(dateTimerInterval));

  return <>
    <div class="input-group">
      <input
        type="text"
        value={ getFilter() }
        placeholder="Type to search"
        onkeyup={ ev => setFilter((ev.target as HTMLInputElement).value) } />
    </div>

    <div class="input-group">
      <input
        type="text"
        placeholder="Type domain to watch"
        value={ getNewDomain() }
        onkeyup={ ev => {
          setNewDomain((ev.target as HTMLInputElement).value);
          setError('');
        } } />

      <button
        disabled={!getNewDomain() || !!getError()}
        onClick={async _ => {
          const { ok, reason } = await watchDomain(getNewDomain());

          if (ok) {
            setNewDomain('');
            props.triggerRowsRefetch();
          } else {
            setError(reason!);
          }
        }}>
        Watch
      </button>
    </div>

    <Show when={getError()}>
      <div class="input-group err">{ getError() }</div>
    </Show>

    <table>
      <thead>
        <tr>
          <th>Domain</th>
          <th>Expiration</th>
          <th>Query ago</th>
          <th></th>
        </tr>
      </thead>

      <tbody>
        <For each={filterRows(props.rows, getFilter())}>
          { row => <DomainTableRow
            entry={row} nowDateAccessor={nowDate}
            unwatchCallback={x => unwatchDomain(x.domain).then(props.triggerRowsRefetch)} /> }
        </For>
      </tbody>
    </table>
  </>;
}
