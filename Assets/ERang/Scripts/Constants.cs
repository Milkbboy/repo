using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ERang
{
    public enum CardType
    {
        None = 0,
        Master,
        Magic, // 마법 카드로 사용 시 해당 카드가 가진 어빌리티를 호출한다. (Ex : 적 공격, 아군 체력 회복, 적 상태 이상 등)
        Individuality, // 마법 카드와 동일한 기능을 하나, 특정 캐릭터만 사용 가능한 전용 마법
        Creature, // 필드에 배치될 크리쳐 카드로 기본적으로 체력, 공격력, 방어력이 존재한다.
        Building, // 건물 카드로 마나와 골드를 소비하여 건물 필드에 배치할 수 있다.
        Charm, // 축복 카드로 해당 카드가 핸드에 들어오면 지정된 어빌리티를 발동시킨다.
        Curse, // 저주 카드로 해당 카드가 핸드에 들어오면 지정된 어빌리티를 발동시킨다.
        Monster, // 적군의 크리쳐 카드로 기본적으로 체력, 공격력, 방어력이 존재한다.
        EnemyMaster, // 적군의 마스터 카드로 기본적으로 체력, 공격력, 방어력이 존재한다.
    }
}