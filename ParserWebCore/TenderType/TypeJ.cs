#region

using System;

#endregion

namespace ParserWebCore.TenderType
{
    public class TypeJ
    {
        public string NotificationNumber { get; set; }
        public string Event { get; set; }
        public DateTime DateTime { get; set; }
        public string TimeZone { get; set; }
        public string TypeFz { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(NotificationNumber)}: {NotificationNumber}, {nameof(Event)}: {Event}, {nameof(DateTime)}: {DateTime}, {nameof(TimeZone)}: {TimeZone}, {nameof(TypeFz)}: {TypeFz}";
        }
    }
}