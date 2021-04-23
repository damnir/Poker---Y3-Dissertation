using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ViewProfile : MonoBehaviour
{
    public TMP_Text userName;
    public TMP_Text cashText;
    public TMP_Text xpText;
    public TMP_Text totalHandsText;
    public TMP_Text TotalHandsWonText;
    public TMP_Text BiggestWinText;
    public TMP_Text friendText;


    public void setValues(string username, string cash, string xp, string totalHands, string totalWins, string biggestWin, bool friends)
    {
        userName.text = username;
        cashText.text = "Cash: $" + cash;
        xpText.text = "Experience: " + xp+"xp";
        totalHandsText.text = "Total Hands Played: " +totalHands;
        TotalHandsWonText.text = "Total Hands Won: " +totalWins;
        BiggestWinText.text = "Biggest Single Win: " +biggestWin;
        if(!friends)
        {

        }
    }

    public void onCloseCLicked()
    {
        Destroy(this.gameObject);
    }


}
