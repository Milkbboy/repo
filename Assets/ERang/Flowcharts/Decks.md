유니티 버전 2022.3.34f1
# 카드 이동
```mermaid
flowchart TB
    Deck -- all card --> HandDeck
    HandDeck -- 크리쳐 카드 --> FieldDeck
    HandDeck -- 건물 카드 --> BuildingDeck
    HandDeck -- HandDeck 에 남은 카드 --> GraveDeck
    FieldDeck
    BuildingDeck
    GraveDeck
```