using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class PlayerController : MonoBehaviourPun
{
    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Components")]
    public Rigidbody rig;

    public int id;
    public Player photonPlayer;

    private int curAttackerId;

    public int curHP;
    public int maxHP;
    public int kills;
    public bool dead;
    private bool flashingDamage;
    public MeshRenderer mr;
    // Rather than one, have a list of potential weapons in inventory to swap through
    public GameObject[] weapons;
    public PlayerWeapon curWeapon;
    // Always start off with first weapon in inventory until another is assigned dynamically (make public since UI needs to see it
    public int currentWeaponIndex = 0;
   

    // Start is called before the first frame update
    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;
        curWeapon = weapons[currentWeaponIndex].GetComponent<PlayerWeapon>();
        Debug.Log(curWeapon);
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        } else
        {
            GameUI.instance.Initialize(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine || dead)
            return;
        Move();
        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        // look for shoot trigger
        if (Input.GetMouseButtonDown(0))
            curWeapon.TryShoot();
        if (Input.GetKeyDown(KeyCode.E))
            photonView.RPC("SwapWeapons", RpcTarget.All);
        if (Input.GetMouseButton(1))
        {
            // Debug.Log(curWeapon.zoomFactor);
            // float FOV = Camera.main.fieldOfView / curWeapon.zoomFactor;
            Camera.main.fieldOfView = 70.5f / curWeapon.zoomFactor;
        }    
        if (Input.GetMouseButtonUp(1))
            Camera.main.fieldOfView = 70.5f;
    }

    // so that everyone can see the updated weapon, make sure this is an rpc call
    [PunRPC]
    public void SwapWeapons()
    {
        weapons[currentWeaponIndex].SetActive(false);
        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        weapons[currentWeaponIndex].SetActive(true);
        curWeapon = weapons[currentWeaponIndex].GetComponent<PlayerWeapon>();

    }
    void Move()
    {
        // get the input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // calculate relative vector based on key input
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        rig.velocity = dir;
    }

    void TryJump()
    {
        // create ray pointing down
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
            return;
        curHP -= damage;
        curAttackerId = attackerId;
        photonView.RPC("DamageFlash", RpcTarget.Others);
        ///update the health bar UI
        GameUI.instance.UpdateHealthBar();
        if (curHP <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
            return;
        StartCoroutine(DamageFlashCoRoutine());
        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;
            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }
    [PunRPC]
    public void GiveAmmo(int ammoToGive)
    {
        curWeapon.GiveAmmo(ammoToGive);
    }
    [PunRPC]
    void Die()
    {
        curHP = 0;
        dead = true;
        GameManager.instance.alivePlayers--;
        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();
        if(photonView.IsMine)
        {
            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);
            GetComponentInChildren<CameraController>().SetAsSpectator();
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }
    [PunRPC]
    public void Heal (int amountToHeal)
    {
        curHP = Mathf.Clamp(curHP + amountToHeal, 0, maxHP);

        //TO DO: Update UI
        GameUI.instance.UpdateHealthBar();
    }

    [PunRPC]
    public void AddKill()
    {
        kills++;
        GameUI.instance.UpdatePlayerInfoText();
    }
}
