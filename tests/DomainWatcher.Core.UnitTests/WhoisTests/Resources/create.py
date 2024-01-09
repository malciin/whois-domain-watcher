import sys
import subprocess
import re

def exec_command(cmd: str):
  result = subprocess.run(cmd, shell=True, stdout=subprocess.PIPE)

  return result.stdout.decode('utf8')


domain = sys.argv[-1]
tld = domain.split('.')[-1]
print(f'Creating domain.response for {domain}')

whoisTldResponse = exec_command(f'whois -h whois.iana.org .{tld}')
whoisTld = re.search(r'whois:\s+([^ ]+)$', whoisTldResponse, flags=re.MULTILINE).groups()[0].strip()
print(f'Whois {whoisTld} is responsible for .{tld}')

print(f'Firing whois -h {whoisTld} {domain}')
response = exec_command(f'whois -h {whoisTld} {domain}')
file = f'{whoisTld}@{domain}.domain.response'

with open(f'{whoisTld}@{domain}.domain.response', encoding='utf8', mode='w') as f:
  f.write(response)

print(f'{file} created')