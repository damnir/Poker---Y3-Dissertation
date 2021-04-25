using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerRe : MonoBehaviour
{
    public enum BetState 
    {
        Wait,
        Fold,
        Check,
        Raise,
        Call,
        Win
    }

    public string netId;
    public string seat;
    public string betGoText;
    public string card1;
    public string card2;
    public GameObject betGo;
    public GameObject foldGo;
    public GameObject animation;
    public GameObject win;
    public GameObject card1Go;
    public GameObject card2Go;
    public GameObject username;


    public void setData(string _netId, string _seat, string _card1, string _card2)
    {
        netId = _netId;
        seat = _seat;
        card1 = _card1;
        card2 = _card2;

        card1Go.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + card1);
        card2Go.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + card2);
        card1Go.SetActive(true);
        card2Go.SetActive(true);
        betGo.SetActive(true);
    }

    void Update()
    {
        betGo.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = betGoText;
    }

    public void updateState(string state, int bet)
    {
        betGo.SetActive(true);
        animation.SetActive(true);

        switch(state)
        {
            case "call":
                if(bet == 0)
                {
                    betGoText = "<sprite=1>Check";
                }
                else
                {
                    betGoText = "<sprite=3>$"+(bet);
                }
                break;
            case "raise":
                betGoText = "<sprite=0>$"+ (bet);
                break;
            case "fold":
                betGoText = "<sprite=2> Fold";
                foldGo.SetActive(true);
                card1Go.SetActive(false);
                card2Go.SetActive(false);
                break;
            case "win":
                betGoText = "<sprite=0>$"+ (bet);
                win.SetActive(true);
                betGo.SetActive(true);
                break;
        }
    }

    public void endTurn()
    {
        animation.SetActive(false);
    }

    public void newRound()
    {
        betGo.SetActive(false);
    }
}
