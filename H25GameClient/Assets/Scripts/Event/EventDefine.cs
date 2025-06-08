using System;
using System.Collections.Generic;

namespace amaz
{
    public class EventDefine
    {
        public static string NETWORK_RECV_DATA = "NETWORK_RECV_DATA";

        public static string GAMEPLAY_1P_READY = "GAMEPLAY_1P_READY";
        public static string GAMEPLAY_2P_READY = "GAMEPLAY_2P_READY";
        
        public static string GAMEPLAY_START = "GAMEPLAY_START";
        public static string GAMEPLAY_PAUSE = "GAMEPLAY_PAUSE";
        public static string GAMELAY_RESUME = "GAMEPLAY_RESUME";

        public static string GAMEPLAY_LOST_1P = "GAMEPLAY_LOST_1P";
        public static string GAMEPLAY_LOST_2P = "GAMEPLAY_LOST_2P";
        
        public static string GAMEPLAY_RESTART = "GAMEPLAY_RESTART";
        
        public static string GAMEPLAY_HIT_TARGET_DEAD = "GAMEPLAY_HIT_TARGET_DEAD";
        public static string GAMEPLAY_HIT_TARGET_SPANW = "GAMEPLAY_HIT_TARGET_SPANW";
        
        public static string GAMEPLAY_MISSION_COMPLETE = "GAMEPLAY_MISSION_COMPLETE";
    }
}
