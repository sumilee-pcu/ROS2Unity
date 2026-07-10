from launch import LaunchDescription
from launch_ros.actions import Node


def generate_launch_description() -> LaunchDescription:
    return LaunchDescription(
        [
            Node(
                package="rosbridge_server",
                executable="rosbridge_websocket",
                name="rosbridge_websocket",
                output="screen",
                parameters=[
                    {
                        "address": "0.0.0.0",
                        "port": 9090,
                        "use_compression": False,
                    }
                ],
            ),
            Node(
                package="ros2unity_examples",
                executable="bridge_probe",
                name="bridge_probe",
                output="screen",
            ),
        ]
    )

