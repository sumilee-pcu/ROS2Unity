#!/usr/bin/env bash
set -eo pipefail

source /opt/ros/jazzy/setup.bash

cd /workspace/ros2_ws
if [[ -f install/setup.bash ]]; then
  source install/setup.bash
else
  colcon build --symlink-install
  source install/setup.bash
fi

ros2 launch ros2unity_examples bridge_demo.launch.py
