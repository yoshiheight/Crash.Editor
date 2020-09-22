using System;
using Crash.Core.UI.Common;

namespace Crash.Core.UI.UIContext
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFont : IDisposable
    {
        CharSize MeasureChar(char ch);
    }
}
