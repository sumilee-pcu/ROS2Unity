using RosSharp.RosBridgeClient;
using UnityEngine;

namespace ROS2Unity
{
    public sealed class BridgeStatusOverlay : MonoBehaviour
    {
        public RosConnector Connector;

        private string latestHeartbeat = "waiting";

        public void SetHeartbeat(string heartbeat)
        {
            latestHeartbeat = heartbeat;
        }

        private void OnGUI()
        {
            bool connected = Connector != null
                && Connector.IsConnected != null
                && Connector.IsConnected.WaitOne(0);

            GUI.Box(new Rect(16f, 16f, 360f, 92f), "ROS2Unity Bridge");
            GUI.Label(new Rect(32f, 44f, 330f, 24f),
                $"rosbridge: {(connected ? "CONNECTED" : "DISCONNECTED")}");
            GUI.Label(new Rect(32f, 68f, 330f, 24f),
                $"heartbeat: {latestHeartbeat}");
        }
    }
}
