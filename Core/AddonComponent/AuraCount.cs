namespace Core;

public readonly struct AuraCount
{
    public int Hash { get; }
    public int PlayerDebuff { get; }
    public int PlayerBuff { get; }
    public int TargetDebuff { get; }
    public int TargetBuff { get; }

    public AuraCount(IAddonDataProvider reader, int cell)
    {
        int hash = reader.GetInt(cell);

        // playerDebuffCount * 1000000 + playerBuffCount * 10000 + targetDebuffCount * 100 + targetBuffCount
        Hash = hash;
        PlayerDebuff = hash / 1000000;
        PlayerBuff = hash / 10000 % 100;
        TargetDebuff = hash / 100 % 100;
        TargetBuff = hash % 100;
    }

    public override string ToString()
    {
        return $"pb: {PlayerBuff} | pd: {PlayerDebuff} | tb: {TargetBuff} | td: {TargetDebuff}";
    }
}
