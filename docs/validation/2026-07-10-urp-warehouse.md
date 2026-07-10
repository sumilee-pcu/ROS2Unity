# Unity URP 물류창고 종단 간 검증

- 검증일: 2026-07-10
- Unity: 2022.3.62f3 Apple Silicon
- 렌더 파이프라인: URP 14.0.12
- ROS: Jazzy / rosbridge_suite 2.7.0
- 브리지: ROS# 2.2.2

## 결과

| 항목 | 결과 |
|---|---|
| ROS# 패키지 해석과 컴파일 | 성공 |
| 공식 Warehouse FBX 로드 | 성공, 렌더러 262개 |
| URP 재질과 정적 충돌체 생성 | 성공 |
| TurtleBot3 Burger 메시 로드 | 성공 |
| macOS Development Player 빌드 | 성공 |
| rosbridge WebSocket 연결 | 성공 |
| `/ros2unity/heartbeat` 수신 | 성공 (`jazzy-ok:29998`) |
| `/cmd_vel` 구독 | 성공 |
| Rigidbody 이동 자동 측정 | 성공 (`0.024m`) |

## 핵심 로그

```text
ROS2UNITY_WAREHOUSE_SETUP_OK
ROS2UNITY_BRIDGE_SMOKE_OK: jazzy-ok:29998
ROS2UNITY_CMD_VEL_SMOKE_OK: distance=0.024m
```

## 다음 검증

1. Windows G14에서 같은 Unity 버전으로 재현한다.
2. `/odom`과 `/tf`를 Unity에서 발행한다.
3. 2D LiDAR를 구현하고 `/scan` 처리량을 측정한다.
