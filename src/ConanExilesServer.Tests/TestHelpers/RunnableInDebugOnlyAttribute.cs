using System.Diagnostics;
using Xunit;

namespace ConanExilesServer.Tests.TestHelpers
{
    public sealed class RunnableInDebugOnlyAttribute : FactAttribute
    {
        public RunnableInDebugOnlyAttribute()
        {
            if (!Debugger.IsAttached)
                Skip = "Can only be run through interactive mode.";
        }
    }
}