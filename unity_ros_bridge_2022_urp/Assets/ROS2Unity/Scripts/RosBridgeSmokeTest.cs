using System;
using UnityEngine;

namespace ROS2Unity
{
    public sealed class RosBridgeSmokeTest : MonoBehaviour
    {
        public HeartbeatSubscriber Heartbeat;
        public DifferentialDriveController Controller;

        [SerializeField] private float timeoutSeconds = 20f;

        private float deadline;
        private bool requireCmdVel;

        private void Start()
        {
            if (!HasCommandLineFlag("-ros2unitySmokeTest"))
            {
                enabled = false;
                return;
            }

            deadline = Time.realtimeSinceStartup + timeoutSeconds;
            requireCmdVel = HasCommandLineFlag("-ros2unityRequireCmdVel");
            Debug.Log("ROS2UNITY_BRIDGE_SMOKE_START");
        }

        private void Update()
        {
            bool heartbeatReady = Heartbeat != null && Heartbeat.HasReceivedHeartbeat;
            bool motionReady = !requireCmdVel
                || (Controller != null && Controller.DistanceMoved >= 0.02f);

            if (heartbeatReady && motionReady)
            {
                Debug.Log("ROS2UNITY_BRIDGE_SMOKE_OK: " + Heartbeat.LatestHeartbeat);
                if (requireCmdVel)
                {
                    Debug.Log("ROS2UNITY_CMD_VEL_SMOKE_OK: distance="
                        + Controller.DistanceMoved.ToString("F3") + "m");
                }
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
