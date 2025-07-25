namespace Core.Session;

public interface IGrindSessionHandler
{
    void Start(string path);
    void Stop(string reason, bool active);
}