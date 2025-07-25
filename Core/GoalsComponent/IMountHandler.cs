using System.Numerics;

namespace Core;

public interface IMountHandler
{
    void MountUp();
    void Dismount();

    bool CanMount();
    bool ShouldMount(Vector3 targetW);
    bool IsMounted();
}
