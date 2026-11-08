# Unity 코드 아카이브

염재빈이 개인·팀 프로젝트에서 작성한 Unity(C#) 스크립트 모음입니다. 프로젝트별로 폴더를 나눴고, 각 폴더에는 본인이 작업한 스크립트만 담았습니다. 에셋·씬·외부 플러그인은 포함하지 않아 이 저장소만으로는 빌드되지 않습니다.

| 폴더 | 프로젝트 | 시기 | 비고 |
|---|---|---|---|
| [Parallel](Parallel) | 3D 모바일 공포게임 (Google Play 출시, 팀 프로젝트) | 2021 | 플레이어 조작·조이스틱, 층 이동, NavMesh 배회 귀신·마네킹 적 AI, 금고 다이얼 퍼즐, 스크린샷 갤러리 |
| [MemoryConvenienceStore](MemoryConvenienceStore) | 메모리편의점, 기억력 미니게임 모음 (모바일) | 2020 | 화면·상태 전환과 사운드 설정(BtnManager), 지도 기억 길찾기 게임(Game_03) |
| [MonsterVillage](MonsterVillage) | 2D 모바일 핵앤슬래시 (팀 프로젝트) | 2025~2026 | 로비 인벤토리·장비 슬롯, 스킬 장착, 능력치 강화, 유저 데이터 |

## 출처와 버전

- Parallel, MemoryConvenienceStore: Google Drive에 보관하던 원본 그대로입니다. `mJoyStick.cs`, `mRotate.cs`는 이전 GitHub 저장소(HorrorGame)와 내용이 같습니다.
- MonsterVillage: Drive에 올려 둔 2026-06~07 버전 대신, 팀 SVN 작업 사본의 최신본(2026-11-08 기준)으로 교체했습니다.
- 메모리편의점의 `BtnManager.cs`에 있던 AdMob 광고 단위 ID는 `YOUR_AD_UNIT_ID`로 바꿔 두었습니다.
