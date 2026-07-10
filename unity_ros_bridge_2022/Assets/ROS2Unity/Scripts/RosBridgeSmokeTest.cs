using System;
using UnityEngine;

namespace ROS2Unity
{
    public sealed class RosBridgeSmokeTest : MonoBehaviour
    {
        public HeartbeatSubscriber Heartbeat;

        [SerializeField] private float timeoutSeconds = 20f;

        private float deadline;

        private void Start()
        {
            if (!HasCommandLineFlag("-ros2unitySmokeTest"))
            {
                enabled = false;
                return;
            }

            deadline = Time.realtimeSinceStartup + timeoutSeconds;
            Debug.Log("ROS2UNITY_BRIDGE_SMOKE_START");
        }

        private void Update()
        {
            if (Heartbeat != null && Heartbeat.HasReceivedHeartbeat)
            {
                Debug.Log("ROS2UNITY_BRIDGE_SMOKE_OK: " + Heartbeat.LatestHeartbeat);
                Application.Quit(0);
                return;
            }

            if (Time.realtimeSinceStartup >= deadline)
            {
                Debug.LogError("ROS2UNITY_BRIDGE_SMOKE_TIMEOUT");
                Application.Quit(2);
            }
        }

        private static bool HasCommandLineFlag(string flag)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            return Array.IndexOf(arguments, flag) >= 0;
        }
    }
}
