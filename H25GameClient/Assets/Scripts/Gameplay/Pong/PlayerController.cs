using UnityEngine;

namespace amaz.gameplay.pong
{
    public abstract class PlayerController
    {
        abstract public bool Up();
        abstract public bool Down();
        abstract public bool Left();
        abstract public bool Right();
    }
}


