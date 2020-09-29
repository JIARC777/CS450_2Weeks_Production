using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    public Slider healthBar;

    public TextMeshProUGUI playerInfoText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI winText;
    public Image winBackground;

    private PlayerController player;

    public static GameUI instance;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void Initialize(PlayerController localPlayer)
    {
        player = localPlayer;
        healthBar.maxValue = player.maxHP;
        healthBar.value = player.curHP;

        UpdatePlayerInfoText();
        UpdateAmmoText();
    }
    // Update is called once per frame
    public void UpdateHealthBar()
    {
        healthBar.value = player.curHP;
    }

    public void UpdatePlayerInfoText()
    {
        playerInfoText.text = "<b>Alive:</b> " + GameManager.instance.alivePlayers + "\n<b>Kills:</b> " + player.kills;
    }

    public void UpdateAmmoText()
    {
        ammoText.text = player.curWeapon.curAmmo + " / " + player.curWeapon.maxAmmo;
    }

    public void SetWinText (string winnerName)
    {
        winBackground.gameObject.SetActive(true);
        winText.text = winnerName + " wins";
    }
}
