using System;
using System.Threading;

namespace SharedLib;

public interface ICancellationTokenSource<T> : IDisposable
{
    CancellationToken Token { get; }

    bool IsCancellationRequested { get; }

    void Cancel();
}

public sealed class CancellationTokenSource<T> :
    CancellationTokenSource, ICancellationTokenSource<T>
{

}
