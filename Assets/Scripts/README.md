# AllocateStats — TitleScene 밑작업

## 파일 구조

```
Scripts/
├── Data/
│   ├── StatData.cs          ← ScriptableObject: 스탯 정의 (이름, 설명 등)
│   └── PlayerStats.cs       ← 싱글턴: 런타임 스탯 보관, 씬 전환 후에도 유지
│
└── Title/
    ├── TitleSceneController.cs ← Phase 흐름 총괄 (Phase 00~04)
    ├── StatAllocatorUI.cs      ← 스탯 바 UI + 포인트 분배 로직
    ├── StatRowUI.cs            ← 스탯 1행 프리팹용 컴포넌트
    ├── TimerUI.cs              ← 20초 타이머 (모래시계 연동)
    ├── NarratorUI.cs           ← 타이핑 애니메이션 + 스탯 설명
    └── TitleAnimator.cs        ← 오브젝트 등장/퇴장/찢기 연출 (stub)
```

---

## 씬 설정 방법

### 1. PlayerStats 준비
- 빈 GameObject `[GameManager]` 생성
- `PlayerStats` 컴포넌트 추가
- DontDestroyOnLoad로 자동 유지됨

### 2. StatData ScriptableObject 생성
- Project 창 우클릭 → Create → AllocateStats → StatData
- 6개 스탯 입력:

| type | displayName | shortName | category | description |
|------|------------|-----------|----------|-------------|
| STR | 근력 | STR | 신체 | 높을수록 눈 앞에 있는 것들을... |
| DEX | 민첩 | DEX | 신체 | 높을수록 재빠르게 이동해... |
| PER | 인식 | PER | 두뇌 | 높을수록 초현실적인 무언가를... |
| INT | 지능 | INT | 두뇌 | 높을수록 어려운 문제를... |
| LUK | 행운 | LUK | 엑스트라 | 소소하게 도움이 됩니다... |
| HUM | 인간성 | HUM | 엑스트라 | 이 곳에서는 쓸모 없습니다. |

### 3. StatRowUI 프리팹 구성
```
StatRow (StatRowUI)
├── LabelTMP         (TextMeshPro)
├── DecrementButton  (Button "-")
├── BarContainer
│    └── BarFill     (Image, Image Type: Filled, Fill Method: Horizontal)
├── IncrementButton  (Button "+")
├── ValueTMP         (TextMeshPro)
└── ScratchObjects[] (선택: 펜 획 스프라이트 5개)
```

### 4. TitleScene 계층 예시
```
TitleScene
├── [GameManager]           → PlayerStats
├── Canvas
│    ├── TitleScreen        → CanvasGroup (Phase 00)
│    ├── NarratorBox        → NarratorUI
│    ├── StatAllocator      → StatAllocatorUI
│    │    └── RowParent     → 행들 자동 생성
│    ├── TimerUI            → TimerUI
│    └── (기타 UI)
└── [TitleController]       → TitleSceneController
                            → TitleAnimator
```

### 5. TitleSceneController Inspector 연결
- `narratorUI` → NarratorUI 컴포넌트
- `statAllocator` → StatAllocatorUI 컴포넌트
- `timerUI` → TimerUI 컴포넌트
- `titleAnimator` → TitleAnimator 컴포넌트
- `titleScreenGroup` → Phase 00 타이틀 CanvasGroup
- `corridorSceneName` → `"CorridorScene"`

---

## Phase 흐름 요약

```
[Phase 00] 타이틀 화면
     ↓ OnGameStartPressed()
[Phase 01] 공책 등장 + 나레이터 타이핑 (15초 이하)
     ↓ 자동
[Phase 02] 구슬/모래시계 소환 + 스탯 분배 (20초 타이머)
     ↓ 타이머 만료              ↓ 확정 버튼
[Phase 03] 구슬 흩어짐 + 랜덤 배분  [Phase 04]
     ↓ 자동                         ↓
[Phase 04] 공책 찢기 + 최종 스탯 확인 + 종이 클릭 → CorridorScene
```

---

## TODO (에셋 완성 후)

- [ ] TitleAnimator의 stub 메서드에 실제 Animator.SetTrigger 연결
- [ ] StatRowUI 스크래치 오브젝트(펜 획 스프라이트) 연결
- [ ] NarratorUI introText / confirmText 텍스트 확정
- [ ] PlayerStats.TOTAL_POINTS 값 조정 (현재 15)
- [ ] TimerUI 모래시계 Animator 파라미터 이름 맞추기 ("Running", "Summon")
