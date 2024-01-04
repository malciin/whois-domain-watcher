## `domain-watcher`

Simple cli http daemon that watches domains and stores whois respones in sqlite database. It can be used with any http tool like `cURL` on linux or `Invoke-RestMethod` on windows `PowerShell`.

### Usage

#### Windows PowerShell - `Invoke-RestMethod`

`Invoke-RestMethod http://localhost:8051` - gets watched domain statuses

`Invoke-RestMethod http://localhost:8051 -Method 'POST' -Body 'google.com'` - watches domain and returns its status

`Invoke-RestMethod http://localhost:8051 -Method 'DELETE' -Body 'google.com'` - unwatches domain

`Invoke-RestMethod http://localhost:8051/queue` - gets queue status - allows to check when each of the watched domains will be queried

#### Linux - `cURL`

`curl localhost:8051` - gets watched domain statuses

`curl -d "google.com" localhost:8051` - watches domain and returns its status

`curl -X DELETE -d "google.com" localhost:8051` - unwatches domain

`curl localhost:8051/queue` - gets queue status - allows to check when each of the watched domains will be queried
