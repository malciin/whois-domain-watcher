# `whois-domain-watcher`

Simple http daemon that watches whois responses for domains. Every whois response is parsed and stored. It can be used for tracking what domains are expired or are about to be expired.

It's intended to be used with any cli http client like `cURL` on linux or `Invoke-RestMethod` on windows `PowerShell`. It also have browser UI.

Built in C# with AOT compilation for daemon, and `solidjs` compiled to single .html file for UI.

## Features

- automatic resolution of responsible whois server for given `TLD` from `whois.iana.org` - e.g. for `.PL` it resolves to `whois.dns.pl`

- auto querying whois for watched domains

- generating list of watched domains sorted by available ones, then by expiration timestamp

- written with `cURL`/`PowerShell` in mind as preferred daemon control API

- simple single .html file UI written in solidjs (only 18KB)

- every whois response is stored in sqlite db which can be processed further

- whole app is built as AOT thanks by using Source Generators rather than reflection

- internal embeddable http server written from scratch for AOT purposes

## Instruction

### Docker

To build docker container use `docker build https://github.com/malciin/whois-domain-watcher.git -t domain-watcher:1.0`

Then run `docker run --rm -it -p XYZ:80 domain-watcher:1.0` where `XYZ` is the port number.

Db is stored in `/app/sqlite.db` (its configurable via DbPath in <a href="https://github.com/malciin/whois-domain-watcher/blob/master/src/DomainWatcher.Cli/settings.Reference.yaml">settings</a>) so you probably want to store it in docker volume.

### Starting daemon

Type `./DomainsWatcher.Cli --port XYZ` where `XYZ` is the port number.

If you don't want to always provide --port XYZ you can create settings.yaml file to configure it. Check <a href="https://github.com/malciin/whois-domain-watcher/blob/master/src/DomainWatcher.Cli/settings.Reference.yaml">settings.Reference.yaml</a> for reference of what options you can configure.

### Usage CLI

For sake of simplicity I've assume port to be `8051` and - if applicable - domain parameter is `google.com`.

| PowerShell | cURL | Description | 
|-|-|-|
| `Invoke-RestMethod http://localhost:8051` | `curl localhost:8051` | Gets watched domains statuses |
| `Invoke-RestMethod http://localhost:8051/google.com` | `curl localhost:8051/google.com` | Gets whois response for any domain. It does not change domain watch status |
| `Invoke-RestMethod http://localhost:8051 -Method 'POST' -Body 'google.com'` | `curl -d "google.com" localhost:8051` | Watches domain and returns its status |
| `Invoke-RestMethod http://localhost:8051 -Method 'DELETE' -Body 'google.com'` | `curl -X DELETE -d "google.com" localhost:8051` | Unwatches domain |
| `Invoke-RestMethod http://localhost:8051/queue` | `curl localhost:8051/queue` | Gets watched domains queue status - allows to check when each of the watched domains will be queried |
| `Invoke-RestMethod http://localhost:8051/s/*.dev` | `curl localhost:8051/s/*.dev` | Searching endpoint. Returns status of any watched domain by given filter. In that example it gets all watched domains for `.dev` tld. More info in `Searching` section |

Examples:

```sh
# Example usage
m@linux:~$ curl localhost:8051
| Domain                  | Status    | Expiration | Queried    |
-----------------------------------------------------------------
| some-untaken-domain.com | Available |            | 1m 17s ago |
| youtube.com             | Taken     | 1mo 5d     | 1m 23s ago |
| google.net              | Taken     | 2mo 4d     | 1m 11s ago |
| twitch.tv               | Taken     | 5mo 4h     | 2d 23h ago |
| microsoft.com           | Taken     | 1y 3mo     | 1m 6s ago  |
| google.com              | Taken     | 4y 8mo     | 1m 28s ago |
m@linux:~$
m@linux:~$ curl localhost:8051/youtube.com
   Domain Name: YOUTUBE.COM
   Registry Domain ID: 142504053_DOMAIN_COM-VRSN
   Registrar WHOIS Server: whois.markmonitor.com
   Registrar URL: http://www.markmonitor.com
   Updated Date: 2023-01-14T09:25:19Z
   Creation Date: 2005-02-15T05:13:12Z
   Registry Expiry Date: 2024-02-15T05:13:12Z
   # rest hidden since its not relevant
m@linux:~$
m@linux:~$ curl localhost:8055/queue
7 domains enqueued.

Query intervals:
    Domain taken                       7d
    Domain taken but expiration hidden 2d
    Domain free                        1d
    Missing parser                     7d
    Base errror retry delay            7d

| Domain                  | Next query | Last query |
-----------------------------------------------------
| some-untaken-domain.com | 23h 55m    | 4m 49s     |
| twitch.tv               | 4d 22m     | 2d 23h     |
| google.com              | 6d 23h     | 5m         |
| youtube.com             | 6d 23h     | 4m 55s     |
| google.net              | 6d 23h     | 4m 42s     |
| microsoft.com           | 6d 23h     | 4m 38s     |
m@linux:~$ 
m@linux:~$ curl -d "dot.net" localhost:8051
dot.net watched! Status: Taken for 1y 15d (2025-01-25 05:00)
m@linux:~$
m@linux:~$ curl localhost:8051/s/*.net
| Domain     | Status | Expiration | Queried     |
--------------------------------------------------
| google.net | Taken  | 2mo 4d     | 44m 27s ago |
| dot.net    | Taken  | 1y 15d     | 11s ago     |
m@linux:~$ 
```

### Usage UI

There are simple browser UI built-in written in solidjs. Open http://localhost:8051 (or whatever port is your daemon listening on) in your browser to see it.

<img src="https://raw.githubusercontent.com/malciin/whois-domain-watcher/master/resources/ui-1.png" />

<img src="https://github.com/malciin/whois-domain-watcher/blob/master/resources/ui-2.png?raw=true" />

## Searching

`/s/{filter}` endpoint supports searching through **watched** domains. Filters could be any letter/digit, `*` or `+`.

`*` means string of any length, `+` means exactly one character. If there are no `*` or `+` character in query, then it matches any string that contains query substring. E.g. filters:

| Url | Result |
|-|-|
| `localhost/s/*.dev` | Domains that ends with `.dev` |
| `localhost/s/*.++` | Domains exactly with two letter tld (`pl`, `eu`, etc.) |
| `localhost/s/*.+++` | Domains exactly with three letter tld |
| `localhost/s/google.*` | Google domains |
| `localhost/s/google*` | Domains that begins with google string |
| `localhost/s/*.***` | Domains with three or more letters tld |
| `localhost/s/+++.*` | 3 letters domains (without tld)  |
| `localhost/s/+++*.*` | 4+ letters domains (without tld)  |
| `localhost/s/gg` | any domain with string gg in it  |
