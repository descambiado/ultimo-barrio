import os
import re

files_to_fix = [
    'Code/UltimoBarrio/Crafting/CraftingStation.cs',
    'Code/UltimoBarrio/Items/HeldConsumable.cs',
    'Code/UltimoBarrio/Items/WorldItemPickup.cs'
]

for filepath in files_to_fix:
    with open(filepath, 'r', encoding='utf-8') as f:
        text = f.read()
    
    # Replace all pairs of double quotes with single double quotes, unless it's just an empty string ""
    # A safe regex: find "" then some characters then ""
    text = re.sub(r'""([^"]+?)""', r'"\1"', text)
    
    # In case there are stray "" like Log.Warning(""..."");
    text = re.sub(r'""(.*?)""', r'"\1"', text)
    
    # If any string ended up as a single quote empty string like string inputs = "; it would be a compile error.
    # The regex ""(.*?)"" turns """" into "". So empty strings become "".
    
    # But let's just do a simple pass: Replace all '""' with '"', then replace '";' with '"";' if there was a syntax error?
    # Wait, the regex '""(.*?)""' works perfectly:
    # """" -> "" (because it matches empty between "")
    # ""hello"" -> "hello"
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(text)
    print(f"Fixed {filepath}")
