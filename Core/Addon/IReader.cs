namespace Core;

public interface IReader
{
    void Update(IAddonDataProvider reader);

    void Reset() { }
}
