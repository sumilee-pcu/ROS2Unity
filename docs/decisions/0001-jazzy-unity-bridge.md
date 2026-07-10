# ADR-0001: Jazzy–Unity 기본 브리지로 ROS# + rosbridge 사용

- 상태: 채택, PoC 검증 전
- 작성일: 2026-07-10

## 배경

ROS 2 Jazzy 기반 교재에서 macOS 또는 Windows의 Unity Editor와 Ubuntu 24.04 컨테이너를 연결해야 한다.

## 결정

첫 교육·개발 경로는 Siemens ROS# 2.2.2와 Jazzy `rosbridge_suite`를 사용한다.

## 이유

- ROS#는 ROS 2, Unity UPM, Unity 2022.3 LTS를 지원한다. 이 저장소의 재현 버전은 Unity 2022.3.62f3과 ROS# 2.2.2로 고정한다.
- `rosbridge_suite`는 Jazzy 패키지로 제공된다.
- Unity 호스트와 ROS 컨테이너를 WebSocket으로 분리할 수 있다.
- 기존 `/cmd_vel`, `/odom`, `/tf`, `/scan` 계약을 재사용할 수 있다.

## 고려한 대안

### Unity ROS-TCP

공식 자료라는 교육적 가치는 있으나 Jazzy 공식 지원과 최신 유지보수가 부족하다. 본문 기본 경로로 채택하지 않는다.

### ros2-for-unity

네이티브 ROS 2 통신과 성능은 장점이다. 공식 배포판 범위가 Galactic/Humble이고 일반 커뮤니티 지원이 제한되므로 Jazzy 소스 포팅을 심화 실험으로 둔다.

### 자체 WebSocket 클라이언트

의존성은 줄일 수 있지만 메시지·서비스·액션·URDF 도구를 다시 구현해야 한다. 초기 교재 범위를 넘어선다.

## 결과

- 제어와 저주기 상태 메시지를 먼저 완성할 수 있다.
- 고대역폭 센서는 성능 한계가 있을 수 있다.
- QoS 의미가 rosbridge를 지나며 동일하게 보존된다고 가정하지 않는다.
- 성능 측정 결과에 따라 센서 데이터 경로만 분리할 수 있다.
