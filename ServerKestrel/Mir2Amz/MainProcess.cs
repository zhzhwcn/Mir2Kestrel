using ServerKestrel.Mir2Amz.Objects;

namespace ServerKestrel.Mir2Amz
{
    internal class MainProcess : IMainProcess
    {
        private DateTime _now = DateTime.MinValue;

        public MainProcess(IGameDataService gameDataService)
        {
            _gameDataService = (GameDataService)gameDataService;
        }

        public DateTime Now => _now;

        private readonly List<Map> _maps = new List<Map>();
        private readonly GameDataService _gameDataService;

        public void Start()
        {
            if (_maps.Any())
            {
                return;
            }

            foreach (var map in _gameDataService.Maps)
            {
                _maps.Add(new Map(map));
            }
        }

        public async Task Process(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
