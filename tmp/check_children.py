
import json
d = json.load(open('Assets/scenes/main.scene', 'r', encoding='utf-8'))
for go in d['GameObjects'][0].get('Children', []):
    print(go.get('Name'))

