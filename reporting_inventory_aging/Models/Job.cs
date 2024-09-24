using System;

namespace reporting_inventory_aging
{
    class Job
    {
        public byte Id { get; set; }
        public string Method { get; set; }
        public string Definition { get; set; }
        public byte Status { get; set; }
        public int Period { get; set; }
        public DateTime Last_dt { get; set; }
        public DateTime Next_dt { get; set; }
        public string Log_path { get; set; }

        public Job(byte _Id, string _Method, string _Definition, byte _Status, int _Period, DateTime _Last_dt, DateTime _Next_dt, string _Log_path)
        {
            Id = _Id;
            Method = _Method;
            Definition = _Definition;
            Status = _Status;
            Period = _Period;
            Last_dt = _Last_dt;
            Next_dt = _Next_dt;
            Log_path = _Log_path;
        }
    }
}
