using RosSharp.RosBridgeClient;
using RosString = RosSharp.RosBridgeClient.MessageTypes.Std.String;
using UnityEngine;

namespace ROS2Unity
{
    public sealed class HeartbeatSubscriber : UnitySubscriber<RosString>
    {
        public BridgeStatusOverlay StatusOverlay;
        public bool HasReceivedHeartbeat { get; private set; }
        public string LatestHeartbeat { get; private set; } = string.Empty;

        private readonly object messageLock = new object();
        private string pendingHeartbeat;

        protected override void ReceiveMessage(RosString message)
        {
            lock (messageLock)
            {
                pendingHeartbeat = message.data;
            }
        }

        private void Update()
        {
            string heartbeat;
            lock (messageLock)
            {
                heartbeat = pendingHeartbeat;
                pendingHeartbeat = null;
            }

            if (!string.IsNullOrEmpty(heartbeat))
            {
                HasReceivedHeartbeat = true;
                LatestHeartbeat = heartbeat;

                if (StatusOverlay != null)
                {
                    StatusOverlay.SetHeartbeat(heartbeat);
                }
            }
        }
    }
}
