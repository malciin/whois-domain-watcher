/* @refresh reload */
import './main.scss';

import { render } from 'solid-js/web';
import { DomainsTable } from './components/domains-table';
import { DomainInfo } from './interfaces/domain-info';
import { Show, createResource } from 'solid-js';
import { fetchWatchedDomains } from './requests';

render(() => {
  const [data, { refetch }] = createResource<DomainInfo[]>(fetchWatchedDomains);

  return <>
    <h1>domain-watcher-ui</h1>

    <Show when={data.latest}>
      <DomainsTable
        rows={data.latest!}
        triggerRowsRefetch={refetch} />
    </Show>
  </>
}, document.getElementById('root')!)
