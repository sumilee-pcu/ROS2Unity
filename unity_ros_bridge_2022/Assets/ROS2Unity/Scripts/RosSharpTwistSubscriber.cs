using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using UnityEngine;

namespace ROS2Unity
{
    public sealed class RosSharpTwistSubscriber : UnitySubscriber<Twist>
    {
        public DifferentialDriveController Controller;

        private readonly object messageLock = new object();
        private float pendingLinearX;
        private float pendingAngularZ;
        private bool hasPendingCommand;

        protected override void ReceiveMessage(Twist message)
        {
            lock (messageLock)
            {
                pendingLinearX = (float)message.linear.x;
                pendingAngularZ = (float)message.angular.z;
                hasPendingCommand = true;
            }
        }

        private void Update()
        {
            float linearX;
            float angularZ;

            lock (messageLock)
            {
                if (!hasPendingCommand)
                {
                    return;
                }

                linearX = pendingLinearX;
                angularZ = pendingAngularZ;
                hasPendingCommand = false;
            }

            if (Controller != null)
            {
                Controller.SetCommand(linearX, angularZ);
            }
        }
    }
}
