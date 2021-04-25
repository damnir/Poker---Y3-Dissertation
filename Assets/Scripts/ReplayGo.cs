using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReplayGo : MonoBehaviour
{
    public DataManager.Game game;
    public TMP_Text rankText;

    // Start is called before the first frame update
    ReplayGo(DataManager.Game _game, string text)
    {
        game = _game;
        rankText.text = text;
    }

    public void onClick()
    {
        ButtonManager.instance.replay();
        GameObject.Find("ReplayMode").GetComponent<ReplayGame>().game = game;
    }

    public void setValues(DataManager.Game _game, string text)
    {
        game = _game;
        rankText.text = text;
    }
}
