
import json
d = json.load(open('tmp/search_asset.json', 'r', encoding='utf-16'))
tools = json.loads(d['result']['content'][0]['text'])['tools']
for t in tools:
    print(t['name'] + ': ' + t['description'])

