using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace ServerKestrel
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class PacketHandleAttribute : Attribute
    {
        public Type HandleType { get; }

        public PacketHandleAttribute(Type handleType)
        {
            HandleType = handleType;
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    internal class PacketHandleAttribute<TPacket> : PacketHandleAttribute where TPacket : Packet
    {
        public PacketHandleAttribute() : base(typeof(TPacket))
        {}
    }
    internal class PacketDispatcher
    {
        private static readonly Dictionary<ClientPacketIds, MethodInfo> ClientPacketHandlers = new();

        private readonly ILogger<PacketDispatcher> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PacketDispatcher(ILogger<PacketDispatcher> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public static void LoadPacketHandlers(IServiceCollection services)
        {
            ClientPacketHandlers.Clear();
            var types = typeof(PacketDispatcher).Assembly.GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public |BindingFlags.Instance);
                foreach (var methodInfo in methods)
                {
                    var handlerInfo = methodInfo.GetCustomAttribute<PacketHandleAttribute>();
                    if (handlerInfo != null)
                    {
                        if (!services.Contains(new ServiceDescriptor(type, type, ServiceLifetime.Singleton)))
                        {
                            services.AddSingleton(type);
                        }

                        if (Activator.CreateInstance(handlerInfo.HandleType) is Packet p && Enum.IsDefined(typeof(ClientPacketIds), p.Index))
                        {
                            var packetId = (ClientPacketIds) p.Index;
                            if (ClientPacketHandlers.ContainsKey(packetId))
                            {
                                throw new Exception("包处理器重复定义:" + packetId);
                            }

                            var parameters = methodInfo.GetParameters();
                            if (parameters.Length > 0)
                            {
                                if (parameters[0].ParameterType != handlerInfo.HandleType)
                                {
                                    throw new Exception($"包处理器所定义参数类型与标记类型不同:[{methodInfo.DeclaringType}.{methodInfo.Name}]");
                                }
                            }

                            if (parameters.Length > 1)
                            {
                                if (parameters[1].ParameterType != typeof(GameContext))
                                {
                                    throw new Exception($"包处理器所定义第二个参数必须为GameContext类型:[{methodInfo.DeclaringType}.{methodInfo.Name}]");
                                }
                            }
                            ClientPacketHandlers[packetId] = methodInfo;
                        }
                            
                    }
                }
            }
        }

        public async ValueTask DispatchPacket(Packet p, GameContext context)
        {
            if (!Enum.IsDefined(typeof(ClientPacketIds), p.Index))
            {
                return;
            }
            var packetId = (ClientPacketIds) p.Index;
            if (!ClientPacketHandlers.ContainsKey(packetId))
            {
                _logger.LogWarning("未找到包处理器：{}", packetId);
                return;
            }

            var method = ClientPacketHandlers[packetId];
            var returnType = method.ReturnType;
            var serviceInstance = _serviceProvider.GetService(method.DeclaringType!);
            if (serviceInstance == null)
            {
                _logger.LogWarning("包处理器未注册服务：{}，[{}.{}]", packetId, method.DeclaringType, method.Name);
                return;
            }

            var parameters = method.GetParameters();
            var realParams = new List<object?>();
            if (parameters.Length > 0)
            {
                realParams.Add(p);
            }

            if (parameters.Length > 1)
            {
                realParams.Add(context);
            }

            if (parameters.Length > 2)
            {
                for (int i = 1; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var isFromService = parameter.GetCustomAttribute<FromServicesAttribute>();
                    if (isFromService != null)
                    {
                        realParams.Add(_serviceProvider.GetService(parameter.ParameterType));
                    }
                    else
                    {
                        realParams.Add(parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null);
                    }
                }
            }
            if (returnType.IsSubclassOf(typeof(Task)))
            {
                var task = (Task?) method.Invoke(serviceInstance, realParams.ToArray());
                if (task != null)
                {
                    await task;
                }
                
            }
            else if (returnType.IsSubclassOf(typeof(ValueTask)))
            {
                var task = (ValueTask?) method.Invoke(serviceInstance, realParams.ToArray());
                if (task != null)
                {
                    await task.Value;
                }
            }
            else
            {
                method.Invoke(serviceInstance, realParams.ToArray());
            }
        }
    }
}
