﻿namespace Behc.Mvp.ToolTipManager
{
    public interface IToolTipManager
    {
        object CurrentToolTip { get; }
        bool IsSuppressed { get; }
        void Suppress(object owner);
        void ReleaseSuppression(object owner);
    }
}