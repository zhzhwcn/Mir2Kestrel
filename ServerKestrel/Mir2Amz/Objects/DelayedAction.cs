namespace ServerKestrel.Mir2Amz.Objects
{
    public class DelayedAction
    {
        public DelayedType Type;
        public long Time;
        public long StartTime;
        public object[] Params;

        public bool FlaggedToRemove;

        public DelayedAction(DelayedType type, long startTime, long time, params object[] p)
        {
            StartTime = startTime;
            Type = type;
            Time = time;
            Params = p;
        }
    }
}
