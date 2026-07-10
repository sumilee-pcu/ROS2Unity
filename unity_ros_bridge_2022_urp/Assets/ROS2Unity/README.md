# ROS2Unity URP project

- Unity: 2022.3.62f3 (macOS Apple Silicon 검증, Windows 동일 버전 권장)
- Render pipeline: Universal Render Pipeline 14.0.12
- ROS bridge: Siemens ROS# tag 2.2.2
- WebSocket endpoint: `ws://localhost:9090`
- Command topic: `/cmd_vel` (`geometry_msgs/msg/Twist`)
- Health topic: `/ros2unity/heartbeat` (`std_msgs/msg/String`)

Start the Jazzy side from the repository root:

```bash
docker compose up -d --build
```

Open `Assets/Scenes/ROS2UnityBridge.unity` for the minimal bridge test or
`Assets/Scenes/ROS2UnityWarehouse.unity` for the warehouse simulation, enter
Play mode, and publish:

```bash
docker compose exec ros2unity bash -lc \
  'source /opt/ros/jazzy/setup.bash && ros2 topic pub -r 10 \
  /cmd_vel geometry_msgs/msg/Twist \
  "{linear: {x: 0.5}, angular: {z: 0.3}}"'
```
