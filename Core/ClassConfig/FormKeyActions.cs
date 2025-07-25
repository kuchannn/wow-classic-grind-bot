using Microsoft.Extensions.Logging;

using System.Collections.Generic;

namespace Core;

public sealed class FormKeyActions : KeyActions
{
    private readonly Dictionary<Form, KeyAction> map = new();

    public override void InitBinds(ILogger logger,
        RequirementFactory factory)
    {
        base.InitBinds(logger, factory);

        foreach (KeyAction keyAction in Sequence)
        {
            map.Add(keyAction.FormValue, keyAction);
        }
    }

    public bool Get(Form form, out KeyAction? val)
        => map.TryGetValue(form, out val);
}