# 기존 PhysicalAI 실습의 Unity 이관표

## 1. 이관 원칙

기존 강의의 ROS 알고리즘 코드는 유지하고, Gazebo가 담당한 가상 환경과 센서만 단계적으로 Unity로 바꾼다. Gazebo Harmonic 실습은 기준선과 비교 대상으로 남긴다.

| 기존 학습 요소 | Unity 후속 구현 | 재사용 여부 |
|---|---|---|
| `/cmd_vel` | 차동구동 Controller | ROS 명령 그대로 사용 |
| `/odom` | Rigidbody pose·velocity 기반 발행 | 메시지 계약 재사용 |
| `/tf` | Unity Transform → ROS 좌표 변환 | 프레임 트리 재사용 |
| `/scan` | Raycast 기반 2D LiDAR | SLAM 입력 재사용 |
| Depth Camera | Unity Camera·Depth texture | PointCloud2 변환 추가 |
| Gazebo world | Unity Scene | 환경 자산 재작성 |
| RViz2 | 기존 RViz2 유지 | 그대로 사용 |
| SLAM Toolbox | Jazzy 컨테이너에서 실행 | 그대로 사용 |
| Nav2 | Jazzy 컨테이너에서 실행 | costmap 파라미터 조정 |
| Q-learning 설명 | ML-Agents 실습으로 확장 | 개념 연결 |

## 2. 권장 개발 순서

1. `/cmd_vel` 구독
2. `/odom` 발행
3. `odom → base_link` TF
4. 고정 `base_link → laser` TF
5. `/scan` 발행
6. RViz2 확인
7. SLAM Toolbox 연결
8. Nav2 연결
9. 카메라·PointCloud2
10. ML-Agents

## 3. 교재에서 유지할 비교 관점

- Gazebo와 Unity의 물리 timestep
- SDF와 Unity Scene/Prefab
- Gazebo 센서 플러그인과 Unity Sensor 컴포넌트
- `ros_gz_bridge`와 rosbridge
- 오른손·왼손 좌표계
- 시뮬레이션 시간과 실제 시간
- 센서 노이즈와 Sim-to-Real

