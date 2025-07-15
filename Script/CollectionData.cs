// ※ 본 코드는 프로젝트에 사용되었던 코드를 기반으로 포트폴리오 용으로 재가공된 예시입니다.

public class CollectionData
{
    #region Fields

    private int id;
    private int level;
    private string groupKey;
    private string typeKey;
    private int rarityRaw;
    private int requiredLevel;
    private string statTypeKey;
    private int statRawValue;

    #endregion

    #region Properties

    public int Id => id;
    public int Level => level;
    public ECollectionCategory Category { get; private set; }
    public ECollectionType Type { get; private set; }
    public ERarity Rarity { get; private set; }
    public int RequiredLevel => requiredLevel;
    public EStatusType StatType { get; private set; }
    public int StatValue { get; private set; }
    public float StatValueFloat { get; private set; }

    #endregion

    public void Initialize()
    {
        ConvertRawValues();
    }

    private void ConvertRawValues()
    {
        Rarity = (ERarity)rarityRaw;
        Category = Enum.Parse<ECollectionCategory>(groupKey);
        Type = Enum.Parse<ECollectionType>(typeKey);
        StatType = Enum.Parse<EStatusType>(statTypeKey);

        if (StatType == EStatusType.DMG_REDU || StatType == EStatusType.CRIT_CH ||
            StatType == EStatusType.ATK_SPD || StatType == EStatusType.MOV_SPD)
        {
            StatValueFloat = statRawValue * 0.01f;
        }
        else
        {
            StatValue = statRawValue;
        }
    }
}
