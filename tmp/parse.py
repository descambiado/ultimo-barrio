
import json
d = json.load(open('tmp/tools.json', 'r', encoding='utf-8'))
tools = json.loads(d['result']['content'][0]['text'])['tools']
for t in tools:
    print(t['name'] + ' (' + t['toolset'] + '): ' + t['description'])

