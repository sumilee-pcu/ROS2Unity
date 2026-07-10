# Jazzy rosbridge 1차 검증 기록

- 검증일: 2026-07-10
- 호스트: Apple Silicon macOS + OrbStack Docker
- 컨테이너 아키텍처: ARM64
- 베이스 이미지: `ros:jazzy-ros-base`
- rosbridge_suite: Jazzy 2.7.0 바이너리 패키지

## 검증 결과

| 항목 | 결과 |
|---|---|
| Docker 이미지 빌드 | 성공 |
| `ros2unity_examples` colcon 빌드 | 성공 |
| `rosbridge_websocket` 9090 기동 | 성공 |
| `/ros2unity/heartbeat` 구독 | 성공 (`jazzy-ok:23`) |
| `/cmd_vel` 발행 | 성공 |
| bridge probe 수신 | 성공 (`linear.x=0.200`, `angular.z=0.500`) |
| Unity Editor 접속 | 미검증 |

## 실제 로그 핵심

```text
Rosbridge WebSocket server started on port 9090
Bridge probe ready: publishing /ros2unity/heartbeat and listening /cmd_vel
cmd_vel received: linear.x=0.200 angular.z=0.500
```

## 후속 검증

1. Unity 2022.3 LTS에 ROS# 2.2.2를 설치한다.
2. `ws://localhost:9090` 연결을 확인한다.
3. Unity에서 heartbeat를 구독한다.
4. ROS의 `/cmd_vel`로 Unity Rigidbody를 제어한다.
5. Unity에서 발행한 메시지를 Jazzy CLI로 확인한다.

