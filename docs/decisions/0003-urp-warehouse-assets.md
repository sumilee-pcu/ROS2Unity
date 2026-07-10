# ADR-0003: URP 물류창고 에셋 통합 방식

- 상태: 채택
- 작성일: 2026-07-10

## 결정

Unity 2022.3.62f3의 URP 프로젝트를 물류창고 실습 주 프로젝트로 사용한다. Unity Robotics Warehouse와 TurtleBot3는 전체 예제 프로젝트를 합치지 않고 필요한 메시 자산만 선별해 포함한다.

## 이유

- 공식 Robotics Warehouse 전체 패키지는 Unity 2020.3, URP 10, Perception 0.9 preview에 의존한다.
- 현재 프로젝트의 URP 14와 구형 패키지를 함께 설치하면 API·셰이더 충돌 가능성이 있다.
- 전체 원본은 약 848MB이지만 실습에 필요한 창고 메시 부분은 약 2.6MB다.
- TurtleBot3의 변환 완료 메시를 사용하면 구형 ROS-TCP 코드와 URDF Importer를 함께 가져오지 않아도 된다.
- 소형 에셋 구성은 이동용 Windows 노트북과 학생용 장비에서 복제·빌드하기 쉽다.

## 라이선스

- Unity Robotics Warehouse: Apache License 2.0
- ROBOTIS TurtleBot3: Apache License 2.0
- 각 에셋 폴더에 원본 URL, 원본 커밋, 라이선스와 제3자 고지를 보존한다.

## 제외 사항

- Unity 2020.3용 Perception randomizer
- 구형 ROS-TCP Connector 설정
- 원본 HDRP 프로젝트
- 고해상도 재질 텍스처 전체

필요한 센서와 도메인 랜덤화 기능은 Unity 2022.3 또는 Unity 6 트랙에서 별도로 구현한다.
