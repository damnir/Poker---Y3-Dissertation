using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : MonoBehaviour
{
    public bool taken = false;

    public bool isTaken(){
        return taken;
    }

    public void setTaken(bool newTaken)
    {
        taken = newTaken;
    }
}
