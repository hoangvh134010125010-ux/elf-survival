using UnityEngine;

public class StaffWeapon : WeaponBase
{
    [Header("Cấu Hình Gậy / Bắn Tầm Xa")]
    public GameObject staffPrefab;
    public Transform firePoint;

    public override void Attack(Transform target, int totalDamage)
    {
        if (!CanAttack() || target == null || staffPrefab == null) return;
        lastAttackTime = Time.time;

        if (PlayerAnimationController.Instance != null)
        {
            PlayerAnimationController.Instance.TriggerAttack();
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Projectile proj = SimpleObjectPool.Spawn<Projectile>(staffPrefab, spawnPos, Quaternion.identity);
        if (proj != null)
        {
            proj.Setup(target, totalDamage);
        }
    }
}
