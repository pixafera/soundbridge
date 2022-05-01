using System;
using System.Collections;

namespace Pixa.Soundbridge
{
    public interface ISoundbridgeListCacheProvider
    {
        Type ChildType { get; }
        SoundbridgeObject Parent { get; }

        IList BuildList(string[] listData);
    }
}