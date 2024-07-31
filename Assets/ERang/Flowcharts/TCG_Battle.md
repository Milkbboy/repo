# 전체 흐름
```mermaid
sequenceDiagram
    GameServer->>WorldLogic: StartTest
    WorldLogic->>WorldLogic: StartBattle
    WorldLogic->>BattleLogic: SetData
    WorldLogic->>BattleLogic: StartBattle
    participant HandCardArea
```