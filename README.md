# `whois-domain-watcher`

Simple http daemon that watches whois responses for domains. Every whois response is parsed and stored. It can be used for tracking what domains are expired or are aboutt to be expired.

It's intended to be used with any cli http client like `cURL` on linux or `Invoke-RestMethod` on windows `PowerShell`.

## Features

- automatic resolution of responsible whois server for given `TLD` from `whois.iana.org` - ex. for `.PL` it resolves to `whois.dns.pl`

- auto querying whois for watched domains

- generating list of watched domains sorted by available ones, then by expiration timestamp

- written with `cURL`/`PowerShell` in mind as preferred daemon control API

- every whois response is stored in sqlite db which can be processed further

- internal http server written from scratch (just for fun)

## Usage

For sake of simplicity I've assume url to be `localhost:8051` and - if applicable - domain parameter is `google.com`.

| PowerShell | cURL | Description | 
|-|-|-|
| `Invoke-RestMethod http://localhost:8051` | `curl localhost:8051` | Gets watched domain statuses |
| `Invoke-RestMethod http://localhost:8051/google.com` | `curl localhost:8051/google.com` | Gets whois response for any domain. It does not change domain watch status |
| `Invoke-RestMethod http://localhost:8051 -Method 'POST' -Body 'google.com'` | `curl -d "google.com" localhost:8051` | Watches domain and returns its status |
| `Invoke-RestMethod http://localhost:8051 -Method 'DELETE' -Body 'google.com'` | `curl -X DELETE -d "google.com" localhost:8051` | Unwatches domain |
| `Invoke-RestMethod http://localhost:8051/queue` | `curl localhost:8051/queue` | Gets processing queue status - allows to check when each of the watched domains will be queried |
