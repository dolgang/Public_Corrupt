// ※ 본 코드는 프로젝트에 사용되었던 코드를 기반으로 포트폴리오 용으로 재가공된 예시입니다.

public class CollectionInfo
{
    private int collectionId;
    private int currentLevel;
    private bool upgradeAvailable;

    private List<CollectionData> levelDataList;
    private CollectionData currentData;

    public event Action<bool> OnUpgradePossible;
    public event Action<int> OnUpgraded;

    public int Id => collectionId;
    public int Level => currentLevel;
    public bool IsUpgradeable
    {
        get => upgradeAvailable;
        private set
        {
            upgradeAvailable = value;
            OnUpgradePossible?.Invoke(value);
        }
    }

    public int MaxLevel => levelDataList.Count - 1;

    public CollectionInfo(int id)
    {
        this.collectionId = id;
        levelDataList = new List<CollectionData>();
    }

    public void AddLevelData(CollectionData data)
    {
        levelDataList.Add(data);
    }

    public void Initialize()
    {
        levelDataList.Sort((x, y) => x.CollectionLevel.CompareTo(y.CollectionLevel));
        foreach (var data in levelDataList)
        {
            data.Initialize();
        }

        currentData = GetCurrentData();
        RegisterEvents();
    }

    public CollectionData GetCurrentData()
    {
        return levelDataList[currentLevel];
    }

    public CollectionData GetNextData()
    {
        if (MaxLevel <= currentLevel) return levelDataList[currentLevel];
        return levelDataList[currentLevel + 1];
    }

    public void ApplyCollectionUpgrade(int id)
    {
        currentLevel++;
        OnUpgraded?.Invoke(id);
        if (currentLevel < MaxLevel) RegisterEvents();
        SaveProgress();
        RecheckUpgradeAvailability();
    }

    private void RegisterEvents()
    {
        if (currentData.CollectionCategory == ECollectionCategory.Equipment)
        {
            CollectionManager.instance.OnSkillCollectionCheck -= EvaluateCondition;
            CollectionManager.instance.OnEquipmentCollectionCheck += EvaluateCondition;
        }
        else if (currentData.CollectionCategory == ECollectionCategory.Skill)
        {
            CollectionManager.instance.OnEquipmentCollectionCheck -= EvaluateCondition;
            CollectionManager.instance.OnSkillCollectionCheck += EvaluateCondition;
        }
    }

    private void UnregisterEvents()
    {
        CollectionManager.instance.OnSkillCollectionCheck -= EvaluateCondition;
        CollectionManager.instance.OnEquipmentCollectionCheck -= EvaluateCondition;
    }

    private List<int> GetMatchingLevels(CollectionData data)
    {
        List<int> levels = new();

        switch (data.CollectionType)
        {
            case ECollectionType.Weapon:
                var weapons = EquipmentManager.instance.GetRarityWeapons(data.Rarity);
                levels = weapons.Select(x => x.enhancementLevel).ToList();
                break;
            case ECollectionType.Armor:
                var armors = EquipmentManager.instance.GetRarityArmors(data.Rarity);
                levels = armors.Select(x => x.enhancementLevel).ToList();
                break;
            case ECollectionType.Active:
                var activeSkills = SkillManager.instance.GetSkillsOnRarity(data.Rarity)
                    .Where(s => s is ActiveSkillData || s is BuffSkillData).ToList();
                levels = activeSkills.Select(s => s.levelFrom0).ToList();
                break;
            case ECollectionType.Passive:
                var passiveSkills = SkillManager.instance.GetSkillsOnRarity(data.Rarity)
                    .OfType<PassiveSkillData>().ToList();
                levels = passiveSkills.Select(x => x.levelFrom0).ToList();
                break;
        }

        return levels;
    }

    public void RecheckUpgradeAvailability()
    {
        var data = GetNextData();
        var levels = GetMatchingLevels(data);
        EvaluateUpgradeCondition(levels, data.LevelCondition);
    }

    private void EvaluateCondition(ERarity rarity, List<Equipment> equipmentList)
    {
        if (currentData.Rarity != rarity) return;

        if (currentData.CollectionType == ECollectionType.Weapon)
        {
            var weapons = equipmentList.OfType<WeaponInfo>().ToList();
            if (weapons.Count > 0)
            {
                var levels = weapons.Select(x => x.enhancementLevel).ToList();
                var nextData = GetNextData();
                EvaluateUpgradeCondition(levels, nextData.LevelCondition);
            }
        }
        else if (currentData.CollectionType == ECollectionType.Armor)
        {
            var armors = equipmentList.OfType<ArmorInfo>().ToList();
            if (armors.Count > 0)
            {
                var levels = armors.Select(x => x.enhancementLevel).ToList();
                var nextData = GetNextData();
                EvaluateUpgradeCondition(levels, nextData.LevelCondition);
            }
        }
    }

    private void EvaluateCondition(ERarity rarity, List<BaseSkillData> skillList)
    {
        if (currentData.Rarity != rarity) return;

        if (currentData.CollectionType == ECollectionType.Active)
        {
            var levels = skillList
                .Where(s => s is ActiveSkillData || s is BuffSkillData)
                .Select(s => s.levelFrom0).ToList();

            if (levels.Count > 0)
            {
                var nextData = GetNextData();
                EvaluateUpgradeCondition(levels, nextData.LevelCondition);
            }
        }
        else if (currentData.CollectionType == ECollectionType.Passive)
        {
            var passiveSkills = skillList.OfType<PassiveSkillData>().ToList();
            if (passiveSkills.Count > 0)
            {
                var levels = passiveSkills.Select(x => x.levelFrom0).ToList();
                var nextData = GetNextData();
                EvaluateUpgradeCondition(levels, nextData.LevelCondition);
            }
        }
    }

    private void EvaluateUpgradeCondition(List<int> levels, int requiredLevel)
    {
        if (currentLevel >= MaxLevel)
        {
            IsUpgradeable = false;
            return;
        }

        foreach (var lv in levels)
        {
            if (lv < requiredLevel)
            {
                IsUpgradeable = false;
                return;
            }
        }

        IsUpgradeable = true;
        UnregisterEvents();
        Debug.Log($"Collection_{collectionId} is ready for upgrade!");
    }
}
