namespace ServerKestrel.Mir2Amz
{
    internal class MainProcess
    {
        private DateTime _now = DateTime.MinValue;
        public DateTime Now => _now;

        public async Task Process(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
