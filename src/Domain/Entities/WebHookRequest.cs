using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Domain.Entities
{
    public class WebHookRequest
    {
        public string SourceKey { get; set; }
        public string RequestorName { get; set; }
        public string EventName { get; set; }
        public DateTime RequestTime { get; set; }
    }
}
