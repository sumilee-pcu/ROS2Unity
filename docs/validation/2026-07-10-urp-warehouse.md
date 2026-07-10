# Unity URP 물류창고 종단 간 검증

- 최초 검증일: 2026-07-10
- 스케일 재검증일: 2026-07-11
- Unity: 2022.3.62f3 Apple Silicon
- 렌더 파이프라인: URP 14.0.12
- ROS: Jazzy / rosbridge_suite 2.7.0
- 브리지: ROS# 2.2.2

## 결과

| 항목 | 결과 |
|---|---|
| ROS# 패키지 해석과 컴파일 | 성공 |
| 공식 Warehouse FBX 로드 | 성공, 렌더러 262개 |
| Unity 미터 단위 통합 | 성공, `1 Unity unit = 1 m` |
| 창고 바운드 | 성공 (`30.89 × 8.25 × 54.86 m`) |
| 랙 바운드 자동 정규화 | 성공 (`2.83 × 3.65 × 0.95 m`) |
| 팔레트와 화물 배치 | 성공, 랙 FBX 원본 적재물 사용 |
| URP 재질과 정적 충돌체 생성 | 성공 |
| TurtleBot3 Burger 메시 단위 | 성공, `0.001` 적용 (`0.18 × 0.19 × 0.14 m`) |
| TurtleBot3 바닥 정렬 | 성공, 메시 하단·콜라이더·가시성 링 기준 일치 |
| 추적 카메라와 가시성 표시 | 성공 |
| 백그라운드 ROS 제어 | 성공 |
| macOS Development Player 빌드 | 성공 |
| rosbridge WebSocket 연결 | 성공 |
| `/ros2unity/heartbeat` 수신 | 성공 (`jazzy-ok:59069`) |
| `/cmd_vel` 구독 | 성공 |
| Rigidbody 이동 자동 측정 | 성공 (`0.021m`, 스케일 재설계 후) |

## 핵심 로그

```text
ROS2UNITY_WAREHOUSE_SETUP_OK
ROS2UNITY_WAREHOUSE_TARGET_BOUNDS: size=(30.89, 8.25, 54.86)
ROS2UNITY_RACK_TARGET_BOUNDS: size=(2.83, 3.65, 0.95)
ROS2UNITY_TURTLEBOT_TARGET_BOUNDS: size=(0.18, 0.19, 0.14)
ROS2UNITY_BRIDGE_SMOKE_OK: jazzy-ok:59069
ROS2UNITY_CMD_VEL_SMOKE_OK: distance=0.021m
```

## 다음 검증

1. Windows G14에서 같은 Unity 버전으로 재현한다.
2. `/odom`과 `/tf`를 Unity에서 발행한다.
3. 2D LiDAR를 구현하고 `/scan` 처리량을 측정한다.
