using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET
{
    [Invoke]
    public class GetAllConfigBytes: AInvokeHandler<ConfigLoader.GetAllConfigBytes, ETTask<Dictionary<Type, byte[]>>>
    {
        public override async ETTask<Dictionary<Type, byte[]>> Handle(ConfigLoader.GetAllConfigBytes args)
        {
            Dictionary<Type, byte[]> output = new Dictionary<Type, byte[]>();
            HashSet<Type> configTypes = CodeTypes.Instance.GetTypes(typeof (ConfigAttribute));
            
            if (Define.IsEditor)
            {
                string ct = "cs";
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
                CodeMode codeMode = globalConfig.CodeMode;
                switch (codeMode)
                {
                    case CodeMode.Client:
                        ct = "c";
                        break;
                    case CodeMode.Server:
                        ct = "s";
                        break;
                    case CodeMode.ClientServer:
                        ct = "cs";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                List<string> startConfigs = new List<string>()
                {
                    "StartMachineConfigCategory", 
                    "StartProcessConfigCategory", 
                    "StartSceneConfigCategory", 
                    "StartZoneConfigCategory",
                };
                foreach (Type configType in configTypes)
                {
                    if (startConfigs.Contains(configType.Name))
                    {
                        // 单机模式下从配置加载源头跳过服务端启动配置，保留原加载逻辑方便后续恢复。
                        /*
                        configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{Options.Instance.StartConfig}/{configType.Name}.bytes";
                        output[configType] = File.ReadAllBytes(configFilePath);
                        */
                        continue;
                    }

                    string configFilePath;
                    configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{configType.Name}.bytes";
                    output[configType] = File.ReadAllBytes(configFilePath);
                }
            }
            else
            {
                List<string> startConfigs = new List<string>()
                {
                    "StartMachineConfigCategory",
                    "StartProcessConfigCategory",
                    "StartSceneConfigCategory",
                    "StartZoneConfigCategory",
                };
                foreach (Type type in configTypes)
                {
                    if (startConfigs.Contains(type.Name))
                    {
                        // 单机模式下跳过服务端启动配置加载。
                        continue;
                    }

                    TextAsset v = await ResourcesComponent.Instance.LoadAssetAsync<TextAsset>($"{ConstValue.ExcelPackagePath}/Config/Bytes/c/{type.Name}.bytes");
                    output[type] = v.bytes;
                }
            }

            return output;
        }
    }
    
    [Invoke]
    public class GetOneConfigBytes: AInvokeHandler<ConfigLoader.GetOneConfigBytes, ETTask<byte[]>>
    {
        public override async ETTask<byte[]> Handle(ConfigLoader.GetOneConfigBytes args)
        {
            string ct = "cs";
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            CodeMode codeMode = globalConfig.CodeMode;
            switch (codeMode)
            {
                case CodeMode.Client:
                    ct = "c";
                    break;
                case CodeMode.Server:
                    ct = "s";
                    break;
                case CodeMode.ClientServer:
                    ct = "cs";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            List<string> startConfigs = new List<string>()
            {
                "StartMachineConfigCategory", 
                "StartProcessConfigCategory", 
                "StartSceneConfigCategory", 
                "StartZoneConfigCategory",
            };

            string configName = args.ConfigName;

            if (startConfigs.Contains(configName))
            {
                // 单机模式下禁用服务端启动配置单独加载，保留原路径逻辑方便后续恢复。
                /*
                string startConfigFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{Options.Instance.StartConfig}/{configName}.bytes";
                return File.ReadAllBytes(startConfigFilePath);
                */
                throw new Exception($"single-player mode disabled start config load: {configName}");
            }

            string configFilePath = $"{ConstValue.ExcelPackagePath}/Config/Bytes/{ct}/{configName}.bytes";

            await ETTask.CompletedTask;
            return File.ReadAllBytes(configFilePath);
        }
    }
}
