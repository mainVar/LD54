using LD54.Sound;
using UnityEngine;
using Wargon.ezs;

namespace LD54 {
    partial class WeaponShootingSystem : UpdateSystem {
        public override void Update() {
            var dt = Time.deltaTime;
            entities.Each((Entity e, Weapon weapon, ProjectilePerShot projectilePerShot, AttackDelay attackDelay, Owner owner, ShootingSound soundData) => {
                if (weapon.timer > 0) {
                    weapon.timer -= dt;
                }
                else {
                    if (Input.GetMouseButton(0)) {

                        for (int i = 0; i < projectilePerShot.value; i++) {
                            var rot = Quaternion.Euler(0, 0, weapon.firePoint.eulerAngles.z + Random.Range(-weapon.spread, weapon.spread));
                            var bullet = ObjectPool.ReuseEntity(weapon.projectile, weapon.firePoint.position, Quaternion.identity);
                            ref var dir = ref bullet.GetRef<Direction>();
                            dir.value = (rot * Vector3.right).normalized;
                            bullet.Get<TransformComponent>().rotation = rot;
                            bullet.SetOwner(owner.Value);
                            
                        }
                        var muzzle = ObjectPool.ReuseEntity(weapon.muzzle, weapon.firePoint.position, weapon.firePoint.rotation);
                        var scale = Random.Range(0.8f, 1.2f);
                        ref var muzzleT = ref muzzle.Get<TransformRef>();
                        
                        muzzleT.value.localScale= new Vector3(scale, Random.value > 0.5f ? scale : -scale, 1);
                        SoundManager.Instance.PlaySound(soundData.SoundData);
                        weapon.timer = attackDelay.value;
                    }
                }
            });
        }
    }
}