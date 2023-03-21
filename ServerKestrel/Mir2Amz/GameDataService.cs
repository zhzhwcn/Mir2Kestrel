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

        public List<Map> Maps { get; private set; } = new();

        private readonly ILogger<GameDataService> _logger;


        public void LoadGameData()
        {
            var envDir = Path.Combine(AycqServerFolder, "Mir200", "Envir");
            var mapDataFile = Path.Combine(envDir, "MapInfo.txt");
            if (!File.Exists(mapDataFile))
            {
                throw new Exception("地图信息不存在");
            }

            var mapData = File.ReadAllLines(mapDataFile, Encoding.GetEncoding("GB2312"));
            var mapDic = new Dictionary<string, Map>();
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
                    var mapInfoData = mapInfo.Split(new char[]{' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (mapInfoData.Length < 2)
                    {
                        _logger.LogWarning("地图信息第{}行可用数据过少", index);
                        continue;
                    }

                    var mapFileName = $"{mapInfoData[0]}.map";
                    var mapName = mapInfoData[1];
                    var extraInfo = line.Substring(infoEndIndex + 1).Trim();

                    mapDic[mapInfoData[0]] = new Map(mapInfoData[0], extraInfo)
                    {
                        FileName = mapFileName,
                        Title = mapName,
                    };
                }
            }
            _logger.LogInformation("已加载{}个地图", mapDic.Count);
            foreach (var kv in mapDic)
            {
                if (string.IsNullOrEmpty(kv.Value.ExtraInfo))
                {
                    continue;
                }
                var extras = kv.Value.ExtraInfo.Split(new char[]{' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
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
                            if (!mapDic.ContainsKey(param))
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
        }
    }
}
