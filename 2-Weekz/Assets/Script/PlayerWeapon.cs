using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum weaponType
{
    handgun,
    railgun
}

public class PlayerWeapon : MonoBehaviourPun
{
    public weaponType weaponVariant;
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;
    public float chargeTime;
    public float zoomFactor;

    private float lastShootTime;

    public PlayerController operatingPlayer;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    //private PlayerController player;
    private void Awake()
    {
      //  player = GetComponent<PlayerController>();
    }
    // Start is called before the first frame update
    public void TryShoot()
    {
        // Test to see if you can shoot
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
            return;
        curAmmo--;
        lastShootTime = Time.time;

        //TO DO:  update ammo UI
        GameUI.instance.UpdateAmmoText();
        // spawn bullet
        photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.transform.position, Camera.main.transform.forward);
    }

    [PunRPC]
    void SpawnBullet(Vector3 pos, Vector3 dir)
    {
        // Spawn and orient
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;
        //Debug.Log(bulletObj);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        // initialize and set velocity
        bulletScript.Initialize(damage, operatingPlayer.id, operatingPlayer.photonView.IsMine);
        bulletScript.rig.velocity = dir * bulletSpeed;
    }
    
    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        // TO DO: Update Ammo UI 
        GameUI.instance.UpdateAmmoText();
    }
}
