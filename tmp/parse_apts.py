
import json
d = json.load(open('tmp/find_apartments5.json', 'r', encoding='utf-16'))
result = d['result']['structuredContent']
print('Total: ' + str(result.get('Total')))
for r in result.get('Results', []):
    print(r['Name'] + ' : ' + r['Id'])

