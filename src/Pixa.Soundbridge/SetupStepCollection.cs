using System.Collections.Generic;

namespace Pixa.Soundbridge
{
    public class SetupStepCollection : System.Collections.ObjectModel.ReadOnlyCollection<SetupStep>
    {
        public SetupStepCollection(IList<SetupStep> list) : base(list)
        {
        }
    }
}