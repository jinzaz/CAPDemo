﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.EventBus.Events
{
    public record IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime createDate) 
        {
            Id = id;
            CreationDate = createDate;
        }

        [JsonInclude]
        public Guid Id { get; private init; }
        [JsonInclude]
        public DateTime CreationDate { get; private init; }
    }
}
