#!/usr/bin/env bash
set -eo pipefail

source /opt/ros/jazzy/setup.bash
cd "$(dirname "$0")/../ros2_ws"

rm -rf build install log
colcon build --symlink-install
source install/setup.bash

python3 -m compileall src/ros2unity_examples/ros2unity_examples
ros2 pkg executables ros2unity_examples
