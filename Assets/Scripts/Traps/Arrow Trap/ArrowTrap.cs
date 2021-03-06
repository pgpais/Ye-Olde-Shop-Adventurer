using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [SerializeField] Arrow arrowPrefab;
    [SerializeField] AudioClip onFireSound;

    [SerializeField] bool isAutomatic;

    [SerializeField] float projectileInitialVelocity = 5f;
    [SerializeField] float shotCooldown = 0.5f;
    [ShowIf("@isAutomatic")]
    [SerializeField] float initialWaitTime = 0f;

    [SerializeField] Transform shootingPoint;
    [SerializeField] float range;
    [SerializeField] LayerMask hitLayer;

    Room room;

    private float nextShotTime;
    bool canMakeSound = false;

    private void Awake()
    {
        nextShotTime = Time.time;
    }

    private void Start()
    {
        room = GetComponentInParent<Room>();

        room.OnPlayerEntersRoomForTheFirstTime.AddListener(OnPlayerEntersRoom);
        room.OnPlayerEntersRoom.AddListener(OnPlayerEntersRoom);
        room.OnPlayerExitsRoom.AddListener(OnPlayerExitsRoom);

        if (isAutomatic)
            StartCoroutine(AutoShootLoop());
    }

    private void OnPlayerExitsRoom()
    {
        canMakeSound = false;
    }

    private void OnPlayerEntersRoom()
    {
        canMakeSound = true;
    }

    private IEnumerator AutoShootLoop()
    {
        yield return new WaitForSeconds(initialWaitTime);

        while (true)
        {
            yield return new WaitForSeconds(shotCooldown);
            Shoot();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isAutomatic)
            return;

        if (Time.time >= nextShotTime)
        {
            RaycastHit2D hit = Physics2D.Raycast(shootingPoint.position, shootingPoint.right, range, hitLayer);
            var gameObj = hit.collider.gameObject;
            if (hit.collider != null && gameObj.CompareTag("Player"))
            {
                Shoot();
                nextShotTime = Time.time + shotCooldown;
            }
        }
    }

    void Shoot()
    {
        Instantiate(arrowPrefab, shootingPoint.position, shootingPoint.rotation).Init(projectileInitialVelocity);
        if (canMakeSound)
        {
            AudioSource.PlayClipAtPoint(onFireSound, transform.position);
        }
    }
}
