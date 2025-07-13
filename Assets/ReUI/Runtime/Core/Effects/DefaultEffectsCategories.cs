using System.Collections.Generic;

namespace Abyse.ReUI
{
    public static class DefaultEffectsCategories
    {
        public const string Open = "Open";
        public const string Close = "Close";
        public const string OpenForwardCloseBackward = "Open Forward Close Backward";

        public static IEnumerable<string> Categories => new[] { Open, Close, OpenForwardCloseBackward };
    }
}