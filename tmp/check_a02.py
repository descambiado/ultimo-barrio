
import json
d = json.load(open('Assets/scenes/main.scene', 'r', encoding='utf-8'))
for go in d['GameObjects'][0].get('Children', []):
    if 'A02' in go.get('Name', ''):
        comps = [c.get('__type') for c in go.get('Components', [])]
        print(go.get('Name') + ' components: ' + str(comps))

