import os

def replace_in_file(path, replacements):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    for old, new in replacements:
        content = content.replace(old, new)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

base_dir = 'Code/UltimoBarrio'

# AIBase
replace_in_file(os.path.join(base_dir, 'AI/AIBase.cs'), [
    ('OnTakeDamage( amount, position, force, attackerId );', 'OnTakeDamage( damageEvent.Amount, damageEvent.Position, damageEvent.Force, System.Guid.Parse(damageEvent.AttackerId) );')
])

# HealthComponent
replace_in_file(os.path.join(base_dir, 'Combat/HealthComponent.cs'), [
    ('OnTakeDamage( amount, position, force, attackerId );', 'OnTakeDamage( damageEvent.Amount, damageEvent.Position, damageEvent.Force, System.Guid.Parse(damageEvent.AttackerId) );'),
    ('if ( amount <= 0 )', 'if ( damageEvent.Amount <= 0 )')
])

# StashComponent
replace_in_file(os.path.join(base_dir, 'Inventory/StashComponent.cs'), [
    ('playerId', 'request.InteractorId')
])

# WorldItemPickup
replace_in_file(os.path.join(base_dir, 'Items/WorldItemPickup.cs'), [
    ('playerId', 'request.InteractorId')
])

# PlayerInteractor
replace_in_file(os.path.join(base_dir, 'PlayerInteractor.cs'), [
    ('interactable = pickup as IInteractable;', 'interactable = pickup.GetComponent<IInteractable>();'),
    ('pickup.GetInteractionPrompt()', 'pickup.GetComponent<IInteractable>().GetInteractionPrompt(new InteractionRequest { InteractorId = player.GameObject.Network.OwnerId.ToString(), InteractorObject = player.GameObject })')
])

# BaseCombatWeapon
replace_in_file(os.path.join(base_dir, 'Combat/BaseCombatWeapon.cs'), [
    ('damageable.TakeDamage( Damage, tr.HitPosition, force, ownerId );', 'damageable.TakeDamage( new DamageEvent { Amount = Damage, Position = tr.HitPosition, Force = force, AttackerId = ownerId.ToString() } );')
])

print('Compile fix 2 done.')
