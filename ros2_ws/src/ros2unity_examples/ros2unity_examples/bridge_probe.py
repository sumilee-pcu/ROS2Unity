"""Publish a heartbeat and report velocity commands arriving from Unity."""

from geometry_msgs.msg import Twist
import rclpy
from rclpy.node import Node
from std_msgs.msg import String


class BridgeProbe(Node):
    """Small observable node for the first bidirectional bridge exercise."""

    def __init__(self) -> None:
        super().__init__("bridge_probe")
        self._sequence = 0
        self._heartbeat = self.create_publisher(
            String,
            "/ros2unity/heartbeat",
            10,
        )
        self._cmd_vel = self.create_subscription(
            Twist,
            "/cmd_vel",
            self._on_cmd_vel,
            10,
        )
        self._timer = self.create_timer(1.0, self._publish_heartbeat)
        self.get_logger().info(
            "Bridge probe ready: publishing /ros2unity/heartbeat and listening /cmd_vel"
        )

    def _publish_heartbeat(self) -> None:
        message = String()
        message.data = f"jazzy-ok:{self._sequence}"
        self._heartbeat.publish(message)
        self._sequence += 1

    def _on_cmd_vel(self, message: Twist) -> None:
        self.get_logger().info(
            "cmd_vel received: linear.x=%.3f angular.z=%.3f"
            % (message.linear.x, message.angular.z)
        )


def main(args=None) -> None:
    rclpy.init(args=args)
    node = BridgeProbe()
    try:
        rclpy.spin(node)
    except KeyboardInterrupt:
        pass
    finally:
        node.destroy_node()
        rclpy.shutdown()


if __name__ == "__main__":
    main()

