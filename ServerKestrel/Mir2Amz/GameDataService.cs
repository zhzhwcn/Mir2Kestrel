using System.Drawing;
using System.Text;
using ServerKestrel.Mir2Amz.Models;

namespace ServerKestrel.Mir2Amz
{
    public class GameDataService : IGameDataService
    {
        private static readonly string AycqServerFolder = "E:\\dotnet\\Mir2\\Mir2\\aycq";

        public GameDataService(ILogger<GameDataService> logger)
        {
            _logger = logger;
        }

        private Dictionary<string, Map> _mapDic = new();
        public ICollection<Map> Maps => _mapDic.Values;

        private readonly ILogger<GameDataService> _logger;

        private void LoadMaps()
        {
            var dataSeparator = new char[] {' ', '\t'};
            var mapDataFile = Path.Combine(AycqServerFolder, "Mir200", "Envir", "MapInfo.txt");
            if (!File.Exists(mapDataFile))
            {
                throw new Exception("地图信息不存在");
            }

            var mapData = File.ReadAllLines(mapDataFile, Encoding.GetEncoding("GB2312"));
            var movementsData = new List<string>();
            for (var index = 0; index < mapData.Length; index++)
            {
                var line = mapData[index];
                if (line.StartsWith(';'))
                {
                    continue;
                }

                if (line.StartsWith('['))
                {
                    var infoEndIndex = line.IndexOf(']');
                    if (infoEndIndex == -1)
                    {
                        _logger.LogWarning("地图信息第{}行格式错误未匹配[]", index);
                        continue;
                    }

                    var mapInfo = line.Substring(1, infoEndIndex - 1);
                    var mapInfoData = mapInfo.Split(dataSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (mapInfoData.Length < 2)
                    {
                        _logger.LogWarning("地图信息第{}行可用数据过少", index);
                        continue;
                    }

                    var mapIndex = mapInfoData[0];
                    var mapFileName = $"{mapInfoData[0]}.map";
                    if (mapIndex.Contains('|'))
                    {
                        var mapIndexData = mapIndex.Split('|');
                        mapIndex = mapIndexData[0];
                        mapFileName = $"{mapIndexData[1]}.map";
                    }
                    var mapName = mapInfoData[1];
                    var extraInfo = line.Substring(infoEndIndex + 1).Trim();

                    _mapDic[mapIndex] = new Map(mapIndex, extraInfo)
                    {
                        FileName = mapFileName,
                        Title = mapName,
                    };
                }
                else if(line.Contains("->"))
                {
                    movementsData.Add(line.Replace(',', ' '));
                }
            }

            _logger.LogInformation("已加载{}个地图", _mapDic.Count);
            if (_mapDic.Count == 0)
            {
                return;
            }

            foreach (var kv in _mapDic)
            {
                if (string.IsNullOrEmpty(kv.Value.ExtraInfo))
                {
                    continue;
                }
                var extras = kv.Value.ExtraInfo.Split(dataSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var extra in extras)
                {
                    var prop = extra;
                    var param = string.Empty;
                    var paramsIndex = extra.IndexOf('(');
                    if (paramsIndex != -1)
                    {
                        prop = extra.Substring(0, paramsIndex);
                        param = extra.Substring(paramsIndex + 1, extra.Length - prop.Length - 2);
                    }

                    switch (prop)
                    {
                        case "NORANDOMMOVE":
                            kv.Value.NoRandom = true;
                            break;
                        case "NORECONNECT":
                            if (!_mapDic.ContainsKey(param))
                            {
                                _logger.LogWarning("未找到重连地图：{}", param);
                                break;
                            }
                            kv.Value.NoReconnect = true;
                            break;
                        case "DARK":
                            kv.Value.Light = LightSetting.Night;
                            break;
                        case "NORECALL":
                            kv.Value.NoRecall = true;
                            break;
                        case "SAFE":
                            kv.Value.NoFight = true;
                            break;
                        case "FIGHT":
                            kv.Value.Fight = true;
                            break;
                        case "DAY":
                            kv.Value.Light = LightSetting.Day;
                            break;
                        case "NODROPITEM":
                            kv.Value.NoDropPlayer = true;
                            break;
                        case "NOPOSITIONMOVE":
                            kv.Value.NoPosition = true;
                            break;
                        case "NEEDHOLE":
                            kv.Value.NeedHole = true;
                            break;
                        case "MINE":
                            kv.Value.MineIndex = 1;
                            break;
                        case "NOTALLOWUSEITEMS":
                        case "NOTALLOWUSEMAGIC":
                        case "CHECKQUEST":
                            //TODO:
                            break;
                        case "ALLOWUSEMYSHOP":
                        case "MUSIC":
                        case "":
                            break;
                        default:
                            _logger.LogWarning("{}({})属性未处理", prop, param);
                            break;
                    }
                }
            }

            if (movementsData.Count > 0)
            {
                foreach (var line in movementsData)
                {
                    var data = line.Split("->", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (data.Length < 2)
                    {
                        continue;
                    }

                    var sourceData = data[0].Split(dataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var targetData = data[1].Split(dataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (sourceData.Length < 3 || targetData.Length < 3)
                    {
                        _logger.LogWarning("{}地图连接未处理", line);
                        continue;
                    }

                    if (!_mapDic.ContainsKey(sourceData[0]))
                    {
                        _logger.LogWarning("{}源地图未找到", line);
                        continue;
                    }

                    if (!_mapDic.ContainsKey(targetData[0]))
                    {
                        _logger.LogWarning("{}目标地图未找到", line);
                        continue;
                    }

                    if (!int.TryParse(sourceData[1], out var sx) || !int.TryParse(sourceData[2], out var sy))
                    {
                        _logger.LogWarning("{}源地图坐标处理失败", line);
                        continue;
                    }

                    if (!int.TryParse(targetData[1], out var tx) || !int.TryParse(targetData[2], out var ty))
                    {
                        _logger.LogWarning("{}目标地图坐标处理失败", line);
                        continue;
                    }

                    _mapDic[sourceData[0]].Movements.Add(new Movement(targetData[0])
                    {
                        Source = new Point(sx, sy),
                        Destination = new Point(tx, ty),
                        NeedHole = _mapDic[targetData[0]].NeedHole,
                    });
                }
            }

            var safePointDataFile = Path.Combine(AycqServerFolder, "Mir200", "Envir", "StartPoint.txt");
            if (File.Exists(safePointDataFile))
            {
                var lines = File.ReadAllLines(safePointDataFile, Encoding.GetEncoding("GB2312"));
                for (var index = 0; index < lines.Length; index++)
                {
                    var line = lines[index];
                    if (line.StartsWith(';'))
                    {
                        continue;
                    }

                    var data = line.Split(dataSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (data.Length < 5)
                    {
                        _logger.LogWarning("{}安全区未处理", line);
                        continue;
                    }

                    if (!_mapDic.ContainsKey(data[0]))
                    {
                        _logger.LogWarning("{}安全区地图未找到", line);
                        continue;
                    }

                    if (!int.TryParse(data[1], out var sx) ||
                        !int.TryParse(data[2], out var sy) ||
                        !ushort.TryParse(data[4], out var size))
                    {
                        _logger.LogWarning("{}安全区数据处理失败", line);
                        continue;
                    }

                    _mapDic[data[0]].SafeZones.Add(new SafeZone()
                    {
                        Location = new Point(sx, sy),
                        Size = size,
                        StartPoint = index <= 1,
                    });
                }
            }
        }


        public void LoadGameData()
        {
            LoadMaps();
        }
    }
}
