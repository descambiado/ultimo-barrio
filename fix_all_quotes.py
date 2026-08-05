import os

directory = 'Code/UltimoBarrio'
for root, _, files in os.walk(directory):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            if '\"\"' in content:
                content = content.replace('\"\"', '\"')
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(content)
                print(f"Fixed quotes in {filepath}")
