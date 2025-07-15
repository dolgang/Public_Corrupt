// ※ 본 코드는 프로젝트에 사용되었던 코드를 기반으로 포트폴리오 용으로 재가공된 예시입니다.

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager instance;

    [SerializeField] private TextAsset collectionDataCSV;

    private Dictionary<int, CollectionInfo> collectionMap;

    public event Action<ERarity, List<Equipment>> OnEquipmentCollectionCheck;
    public event Action<ERarity, List<BaseSkillData>> OnSkillCollectionCheck;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void InitializeCollectionManager()
    {
        collectionMap = new Dictionary<int, CollectionInfo>();
        LoadCollectionData();
        LoadProgress();

        foreach (var pair in collectionMap)
        {
            pair.Value.RecheckUpgradeAvailability();

            if (pair.Value.Level > 0)
                ApplyInitialStatus(pair.Value.GetCurrentData());
        }

        UIManager.instance.TryGetUI<UICollectionPanel>()?.SetCollectionList();
    }

    private void LoadCollectionData()
    {
        var parsedList = CSVSerializer.Deserialize<CollectionData>(collectionDataCSV.text);

        foreach (var data in parsedList)
        {
            if (!collectionMap.ContainsKey(data.Id))
                collectionMap[data.Id] = new CollectionInfo(data.Id);

            collectionMap[data.Id].AddLevelData(data);
        }

        foreach (var pair in collectionMap)
        {
            pair.Value.Initialize();
        }
    }

    public List<int> GetCollectionIds() => collectionMap.Keys.ToList();

    public CollectionInfo GetCollectionInfo(int id)
    {
        collectionMap.TryGetValue(id, out var info);
        return info;
    }

    public int GetCollectionLevel(int id)
    {
        collectionMap.TryGetValue(id, out var info);
        return info?.Level ?? 0;
    }

    public bool HasAnyUpgradeable()
    {
        return collectionMap.Values.Any(info => info.IsUpgradeable);
    }

    public void TriggerUpgradeCheck(Equipment equipment)
    {
        if (equipment is WeaponInfo)
        {
            var weapons = EquipmentManager.instance.GetRarityWeapons(equipment.rarity);
            OnEquipmentCollectionCheck?.Invoke(equipment.rarity, weapons.Cast<Equipment>().ToList());
        }
        else if (equipment is ArmorInfo)
        {
            var armors = EquipmentManager.instance.GetRarityArmors(equipment.rarity);
            OnEquipmentCollectionCheck?.Invoke(equipment.rarity, armors.Cast<Equipment>().ToList());
        }
    }

    public void TriggerUpgradeCheck(BaseSkillData skill)
    {
        var allSkills = SkillManager.instance.GetSkillsOnRarity(skill.rarity);
        var filtered = skill is ActiveSkillData
            ? allSkills.Where(s => s is ActiveSkillData).ToList()
            : allSkills.Where(s => s is PassiveSkillData).ToList();

        OnSkillCollectionCheck?.Invoke(skill.rarity, filtered);
    }

    public void PerformCollectionUpgrade(int id)
    {
        if (!collectionMap.TryGetValue(id, out var info)) return;

        var previous = info.Level > 0 ? info.GetCurrentData() : null;
        info.ApplyCollectionUpgrade(id);
        var next = info.GetCurrentData();

        ApplyStatChange(previous, next);
        PlayerManager.instance.UpdateBattleScore();
    }

    private void ApplyStatChange(CollectionData before, CollectionData after)
    {
        if (before != null)
        {
            if (before.StatusValue != 0)
            {
                if (IsPercentageStat(before.StatusType))
                    PlayerManager.instance.status.ChangePercentStat(before.StatusType, -new BigInteger(before.StatusValue));
                else
                    PlayerManager.instance.status.ChangeBaseStat(before.StatusType, -new BigInteger(before.StatusValue));
            }
            else
            {
                PlayerManager.instance.status.ChangeBaseStat(before.StatusType, -before.StatusValueFloat);
            }
        }

        if (after.StatusValue != 0)
        {
            if (IsPercentageStat(after.StatusType))
                PlayerManager.instance.status.ChangePercentStat(after.StatusType, new BigInteger(after.StatusValue));
            else
                PlayerManager.instance.status.ChangeBaseStat(after.StatusType, new BigInteger(after.StatusValue));
        }
        else
        {
            PlayerManager.instance.status.ChangeBaseStat(after.StatusType, after.StatusValueFloat);
        }
    }

    private void ApplyInitialStatus(CollectionData data)
    {
        if (data.StatusValue != 0)
        {
            if (IsPercentageStat(data.StatusType))
                PlayerManager.instance.status.ChangePercentStat(data.StatusType, new BigInteger(data.StatusValue));
            else
                PlayerManager.instance.status.ChangeBaseStat(data.StatusType, new BigInteger(data.StatusValue));
        }
        else
        {
            PlayerManager.instance.status.ChangeBaseStat(data.StatusType, data.StatusValueFloat);
        }
    }

    private bool IsPercentageStat(EStatusType type)
    {
        return type >= EStatusType.SKILL_DMG && type <= EStatusType.GOLD_INCREASE;
    }
}
