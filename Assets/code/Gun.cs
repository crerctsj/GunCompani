using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("총기 설정")]
    public int maxAmmo = 12;
    public int currentAmmo;
    public float damage = 20f;
    public float fireRate = 0.5f;
    public float spread = 0.05f;

    [Header("연결 객체")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    private bool isReloading = false;
    private float lastFireTime;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        // [핵심] 부모가 없거나(바닥에 있음) 비활성화 상태면 사격 불가
        if (transform.parent == null || !gameObject.activeInHierarchy) return;

        if (isReloading) return;

        // 마우스 왼쪽 클릭 시 발사
        if (Input.GetButtonDown("Fire1") && Time.time >= lastFireTime + fireRate)
        {
            if (currentAmmo > 0) Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        lastFireTime = Time.time;
        currentAmmo--;

        // 탄퍼짐 적용된 방향 계산
        Vector3 shotDirection = firePoint.forward;
        shotDirection += Random.insideUnitSphere * spread;

        // 총알 생성
        Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shotDirection));
        
        Debug.Log($"사격! 잔탄: {currentAmmo}");
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(2f);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}