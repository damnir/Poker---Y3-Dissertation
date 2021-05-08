using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using static MLAPI.Spawning.NetworkSpawnManager;


public class ViewProfile : MonoBehaviour
{
    public TMP_Text userName;
    public TMP_Text cashText;
    public TMP_Text xpText;
    public TMP_Text totalHandsText;
    public TMP_Text TotalHandsWonText;
    public TMP_Text BiggestWinText;
    public TMP_Text friendText;
    public GameObject addButton;
    public GameObject league;
    public TMP_Text leagueText;
    public Sprite silver;
    public Sprite gold;
    public Sprite purple;
    public string netId;


    public void setValues(string username, string cash, string xp, string totalHands, string totalWins, string biggestWin, bool friends, string id)
    {
        userName.text = username;
        cashText.text = "Cash: $" + cash;
        xpText.text = "Experience: " + xp+"xp";
        totalHandsText.text = "Total Hands Played: " +totalHands;
        TotalHandsWonText.text = "Total Hands Won: " +totalWins;
        BiggestWinText.text = "Biggest Single Win: $" +biggestWin;
        netId = id;
        if(!friends)
        {
            addButton.SetActive(true);
        }
        else{
            friendText.text = "[ friends ]";
        }

        int _xp = Int32.Parse(xp);
        if(_xp < 1000)
        {
            league.GetComponent<Image>().sprite = silver;
            leagueText.text = "Silver";
        }
        if(_xp > 1000 && _xp < 3000)
        {
            league.GetComponent<Image>().sprite = gold;
            leagueText.text = "Gold";
        }
        if(_xp > 3000)
        {
            league.GetComponent<Image>().sprite = purple;
            leagueText.text = "Platinum";
        }

    }

    public void onCloseCLicked()
    {
        Destroy(this.gameObject);
    }

    public void onAddClicked()
    {
        Player player = GetLocalPlayerObject().GetComponent<Player>();
        LoginManager.instance.quickAdd(player.username.Value, player.netId.Value, netId);
        Destroy(this.gameObject);
    }


}
