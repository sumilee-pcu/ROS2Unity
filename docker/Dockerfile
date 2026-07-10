FROM ros:jazzy-ros-base

SHELL ["/bin/bash", "-c"]

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       python3-colcon-common-extensions \
       ros-jazzy-rosbridge-suite \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /workspace

COPY ros2_ws /workspace/ros2_ws
COPY scripts /workspace/scripts

RUN chmod +x /workspace/scripts/*.sh \
    && source /opt/ros/jazzy/setup.bash \
    && cd /workspace/ros2_ws \
    && colcon build --symlink-install

EXPOSE 9090

