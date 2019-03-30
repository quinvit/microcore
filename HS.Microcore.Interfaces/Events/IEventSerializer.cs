using System;
using System.Collections.Generic;

namespace HS.Microcore.Interfaces.Events
{

    public interface IEventSerializer
    {
        IEnumerable<SerializedEventField> Serialize(IEvent evt, Func<EventFieldAttribute, bool> predicate = null);
    }
}
