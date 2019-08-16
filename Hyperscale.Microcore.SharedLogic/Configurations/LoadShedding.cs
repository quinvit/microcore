using System;
using Hyperscale.Microcore.Interfaces.Configuration;

namespace Hyperscale.Microcore.SharedLogic.Configurations
{

    [Serializable]
    [ConfigurationRoot("Microcore.LoadShedding", RootStrategy.ReplaceClassNameWithPath)]
    public class LoadShedding : IConfigObject
    {
        public enum Toggle
        {
            Disabled,
            LogOnly,
            Drop,
        }

        public Toggle   DropRequestsByDeathTime   { get; set; } = Toggle.Disabled;
        public TimeSpan RequestTimeToLive         { get; set; } = TimeSpan.FromSeconds(90);
        public TimeSpan TimeToDropBeforeDeathTime { get; set; } = TimeSpan.FromSeconds(5);

        public Toggle   DropMicrocoreRequestsBySpanTime          { get; set; } = Toggle.Disabled;
        public TimeSpan DropMicrocoreRequestsOlderThanSpanTimeBy { get; set; } = TimeSpan.FromSeconds(5);

    }

}
