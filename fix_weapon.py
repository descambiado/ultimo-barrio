import sys

def replace_in_file(file_path, old_text, new_text):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    if old_text not in content:
        print(f'Cannot find text in {file_path}')
        sys.exit(1)
    content = content.replace(old_text, new_text)
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)

weapon_path = r'C:\Users\davyd\.gemini\antigravity\brain\ab476ef3-b11e-42ff-9976-249fc387e4b5\.system_generated\worktrees\subagent-Combat-Developer-self-830044b2\Code\UltimoBarrio\Combat\BaseCombatWeapon.cs'

old1 = '''                    if (Networking.IsHost)
                    {
                        damageable.TakeDamage(BaseDamage, tr.HitPosition, ray.Forward * 100f, Guid.Empty);
                    }
                    else
                    {
                        RpcApplyDamage(tr.GameObject.Id, BaseDamage, tr.HitPosition, ray.Forward * 100f);
                    }'''

new1 = '''                    var dmg = new DamageEvent
                    {
                        Amount = BaseDamage,
                        Position = tr.HitPosition,
                        Force = ray.Forward * 100f,
                        AttackerId = Connection.Local?.Id.ToString() ?? "",
                        WeaponId = GameObject.Name
                    };

                    if (Networking.IsHost)
                    {
                        damageable.TakeDamage(dmg);
                    }
                    else
                    {
                        RpcApplyDamage(tr.GameObject.Id, dmg.Amount, dmg.Position, dmg.Force, dmg.AttackerId, dmg.WeaponId);
                    }'''

old2 = '''        [Rpc.Host]
        private void RpcApplyDamage(Guid hitObjectId, float damage, Vector3 position, Vector3 force)
        {
            var hitObj = Scene.Directory.FindByGuid(hitObjectId);
            if (hitObj != null)
            {
                var damageable = hitObj.Components.GetInAncestorsOrSelf<IDamageable>();
                if (damageable != null)
                {
                    // Check friendly fire later if needed, disabled by default per requirements
                    damageable.TakeDamage(damage, position, force, Guid.Empty);
                }
            }
        }'''

new2 = '''        [Rpc.Host]
        private void RpcApplyDamage(Guid hitObjectId, float damage, Vector3 position, Vector3 force, string attackerId, string weaponId)
        {
            var hitObj = Scene.Directory.FindByGuid(hitObjectId);
            if (hitObj != null)
            {
                var damageable = hitObj.Components.GetInAncestorsOrSelf<IDamageable>();
                if (damageable != null)
                {
                    var dmg = new DamageEvent
                    {
                        Amount = damage,
                        Position = position,
                        Force = force,
                        AttackerId = attackerId,
                        WeaponId = weaponId
                    };
                    // Check friendly fire later if needed, disabled by default per requirements
                    damageable.TakeDamage(dmg);
                }
            }
        }'''

replace_in_file(weapon_path, old1, new1)
replace_in_file(weapon_path, old2, new2)
print('BaseCombatWeapon updated successfully')
