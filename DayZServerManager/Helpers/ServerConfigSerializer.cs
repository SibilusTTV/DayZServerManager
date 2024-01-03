using DayZServerManager.ServerConfigClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DayZServerManager.Helpers
{
    public class ServerConfigSerializer
    {
        public static ServerConfig Deserialize(string config)
        {
            ServerConfig cfg = new ServerConfig();
            DeserializeAllProperties(config, cfg);
            DeserializeMotd(config, cfg);
            return cfg;
        }

        private static void DeserializeAllProperties(string config, ServerConfig cfg)
        {
            if (cfg != null)
            {
                string pattern = "[^\\n\\w]*([a-zA-Z\\[\\]]+)\\s*=\\s*(\"([^\\n\"]*)\"|([-0-9]*)|([Tt]rue|[Ff]alse));";
                Regex reg = new Regex(pattern);
                MatchCollection matches = reg.Matches(config);

                foreach (Match match in matches)
                {
                    switch (match.Groups[1].Value)
                    {
                        case "hostname":
                            cfg.hostname = match.Groups[3].Value;
                            break;
                        case "password":
                            cfg.password = match.Groups[3].Value;
                            break;
                        case "passwordAdmin":
                            cfg.passwordAdmin = match.Groups[3].Value;
                            break;
                        case "enableWhitelist":
                            cfg.enableWhitelist = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disableBanlist":
                            cfg.disableBanlist = bool.Parse(match.Groups[5].Value);
                            break;
                        case "disablePrioritylist":
                            cfg.disablePrioritylist = bool.Parse(match.Groups[5].Value);
                            break;
                        case "maxPlayers":
                            cfg.maxPlayers = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "verifySignatures":
                            cfg.verifySignatures = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "forceSameBuild":
                            cfg.forceSameBuild = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disableVoN":
                            cfg.disableVoN = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "vonCodecQuality":
                            cfg.vonCodecQuality = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "enableCfgGameplayFile":
                            cfg.enableCfgGameplayFile = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disable3rdPerson":
                            cfg.disable3rdPerson = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disableCrosshair":
                            cfg.disableCrosshair = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disablePersonalLight":
                            cfg.disablePersonalLight = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "lightingConfig":
                            cfg.lightingConfig = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "serverTime":
                            cfg.serverTime = match.Groups[3].Value;
                            break;
                        case "serverTimeAcceleration":
                            cfg.serverTimeAcceleration = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "serverNightTimeAcceleration":
                            cfg.serverNightTimeAcceleration = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "serverTimePersistent":
                            cfg.serverTimePersistent = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "guaranteedUpdates":
                            cfg.guaranteedUpdates = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "loginQueueConcurrentPlayers":
                            cfg.loginQueueConcurrentPlayers = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "loginQueueMaxPlayers":
                            cfg.loginQueueMaxPlayers = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "instanceId":
                            cfg.instanceId = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "storageAutoFix":
                            cfg.storageAutoFix = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "steamQueryPort":
                            cfg.steamQueryPort = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "respawnTime":
                            cfg.respawnTime = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "timeStampFormat":
                            cfg.timeStampFormat = match.Groups[3].Value;
                            break;
                        case "logAverageFps":
                            cfg.logAverageFps = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "logMemory":
                            cfg.logMemory = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "logPlayers":
                            cfg.logPlayers = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "logFile":
                            cfg.logFile = match.Groups[3].Value;
                            break;
                        case "adminLogPlayerHitsOnly":
                            cfg.adminLogPlayerHitsOnly = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "adminLogPlacement":
                            cfg.adminLogPlacement = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "adminLogBuildActions":
                            cfg.adminLogBuildActions = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "adminLogPlayerList":
                            cfg.adminLogPlayerList = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disableMultiAccountMitigation":
                            cfg.disableMultiAccountMitigation = bool.Parse(match.Groups[5].Value);
                            break;
                        case "enableDebugMonitor":
                            cfg.enableDebugMonitor = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "allowFilePatching":
                            cfg.allowFilePatching = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "simulatedPlayersBatch":
                            cfg.simulatedPlayersBatch = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "multithreadedReplication":
                            cfg.multithreadedReplication = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "speedhackDetection":
                            cfg.speedhackDetection = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "networkRangeClose":
                            cfg.networkRangeClose = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "networkRangeNear":
                            cfg.networkRangeNear = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "networkRangeFar":
                            cfg.networkRangeFar = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "networkRangeDistantEffect":
                            cfg.networkRangeDistantEffect = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "networkObjectBatchSend":
                            cfg.networkObjectBatchSend = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "networkObjectBatchCompute":
                            cfg.networkObjectBatchCompute = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "defaultVisibility":
                            cfg.defaultVisibility = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "defaultObjectViewDistance":
                            cfg.defaultObjectViewDistance = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disableBaseDamage":
                            cfg.disableBaseDamage = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disableContainerDamage":
                            cfg.disableContainerDamage = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "disableRespawnDialog":
                            cfg.disableRespawnDialog = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "pingWarning":
                            cfg.pingWarning = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "pingCritical":
                            cfg.pingCritical = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "MaxPing":
                            cfg.MaxPing = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "serverFpsWarning":
                            cfg.serverFpsWarning = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "motdInterval":
                            cfg.motdInterval = Int32.Parse(match.Groups[4].Value);
                            break;
                        case "template":
                            cfg.template = match.Groups[3].Value;
                            break;
                    }
                }
            }
        }

        private static void DeserializeMotd(string config, ServerConfig cfg)
        {
            if (cfg != null)
            {
                string pattern = "motd\\[\\]\\s*=\\s*{\\s*([^\\n]*)\\s*};";
                Regex reg = new Regex(pattern);
                Match m = reg.Match(config);
                if (m != null && m.Success)
                {
                    cfg.motd = new List<string>();
                    string pattern2 = "\"(([^\\n\"])*)\"";
                    Regex reg2 = new Regex(pattern2);
                    MatchCollection matches = reg2.Matches(m.Groups[0].Value);
                    
                    foreach (Match match in matches)
                    {
                        cfg.motd.Add(match.Groups[1].Value);
                    }
                }
            }
        }

        public static string Serialize(ServerConfig cfg)
        {
            string returnString = "";

            if (cfg != null)
            {
                returnString += $"hostname = \"{cfg.hostname}\";";
                returnString += $"{Environment.NewLine}password = \"{cfg.password}\";";
                returnString += $"{Environment.NewLine}passwordAdmin = \"{cfg.passwordAdmin}\";";
                returnString += $"{Environment.NewLine}enableWhitelist = {cfg.enableWhitelist.ToString()};";
                returnString += $"{Environment.NewLine}disableBanlist = {cfg.disableBanlist.ToString()};";
                returnString += $"{Environment.NewLine}disablePrioritylist = {cfg.disablePrioritylist.ToString()};";
                returnString += $"{Environment.NewLine}maxPlayers = {cfg.maxPlayers.ToString()};";
                returnString += $"{Environment.NewLine}verifySignatures = {cfg.verifySignatures.ToString()};";
                returnString += $"{Environment.NewLine}forceSameBuild = {cfg.forceSameBuild.ToString()};";
                returnString += $"{Environment.NewLine}disableVoN = {cfg.disableVoN.ToString()};";
                returnString += $"{Environment.NewLine}vonCodecQuality = {cfg.vonCodecQuality.ToString()};";
                returnString += $"{Environment.NewLine}enableCfgGameplayFile = {cfg.enableCfgGameplayFile.ToString()};";
                returnString += $"{Environment.NewLine}disable3rdPerson = {cfg.disable3rdPerson.ToString()};";
                returnString += $"{Environment.NewLine}disableCrosshair = {cfg.disableCrosshair.ToString()};";
                returnString += $"{Environment.NewLine}disablePersonalLight = {cfg.disablePersonalLight.ToString()};";
                returnString += $"{Environment.NewLine}lightingConfig = {cfg.lightingConfig.ToString()};";
                returnString += $"{Environment.NewLine}serverTime = \"{cfg.serverTime}\";";
                returnString += $"{Environment.NewLine}serverTimeAcceleration = {cfg.serverTimeAcceleration.ToString()};";
                returnString += $"{Environment.NewLine}serverNightTimeAcceleration = {cfg.serverNightTimeAcceleration.ToString()};";
                returnString += $"{Environment.NewLine}serverTimePersistent = {cfg.serverTimePersistent.ToString()};";
                returnString += $"{Environment.NewLine}guaranteedUpdates = {cfg.guaranteedUpdates.ToString()};";
                returnString += $"{Environment.NewLine}loginQueueConcurrentPlayers = {cfg.loginQueueConcurrentPlayers.ToString()};";
                returnString += $"{Environment.NewLine}loginQueueMaxPlayers = {cfg.loginQueueMaxPlayers.ToString()};";
                returnString += $"{Environment.NewLine}instanceId = {cfg.instanceId.ToString()};";
                returnString += $"{Environment.NewLine}storageAutoFix = {cfg.storageAutoFix.ToString()};";
                returnString += $"{Environment.NewLine}steamQueryPort = {cfg.steamQueryPort.ToString()};";
                returnString += $"{Environment.NewLine}respawnTime = {cfg.respawnTime.ToString()};";
                returnString += $"{Environment.NewLine}timeStampFormat = {cfg.timeStampFormat};";
                returnString += $"{Environment.NewLine}logAverageFps = {cfg.logAverageFps.ToString()};";
                returnString += $"{Environment.NewLine}logMemory = {cfg.logMemory.ToString()};";
                returnString += $"{Environment.NewLine}logPlayers = {cfg.logPlayers.ToString()};";
                returnString += $"{Environment.NewLine}logFile = \"{cfg.logFile}\";";
                returnString += $"{Environment.NewLine}adminLogPlayerHitsOnly = {cfg.adminLogPlayerHitsOnly.ToString()};";
                returnString += $"{Environment.NewLine}adminLogPlacement = {cfg.adminLogPlacement.ToString()};";
                returnString += $"{Environment.NewLine}adminLogBuildActions = {cfg.adminLogBuildActions.ToString()};";
                returnString += $"{Environment.NewLine}adminLogPlayerList = {cfg.adminLogPlayerList.ToString()};";
                returnString += $"{Environment.NewLine}disableMultiAccountMitigation = {cfg.disableMultiAccountMitigation.ToString()};";
                returnString += $"{Environment.NewLine}enableDebugMonitor = {cfg.enableDebugMonitor.ToString()};";
                returnString += $"{Environment.NewLine}allowFilePatching = {cfg.allowFilePatching.ToString()};";
                returnString += $"{Environment.NewLine}simulatedPlayersBatch = {cfg.simulatedPlayersBatch.ToString()};";
                returnString += $"{Environment.NewLine}multithreadedReplication = {cfg.multithreadedReplication.ToString()};";
                returnString += $"{Environment.NewLine}speedhackDetection = {cfg.speedhackDetection.ToString()};";
                returnString += $"{Environment.NewLine}networkRangeClose = {cfg.networkRangeClose.ToString()};";
                returnString += $"{Environment.NewLine}networkRangeNear = {cfg.networkRangeNear.ToString()};";
                returnString += $"{Environment.NewLine}networkRangeFar = {cfg.networkRangeFar.ToString()};";
                returnString += $"{Environment.NewLine}networkRangeDistantEffect = {cfg.networkRangeDistantEffect.ToString()};";
                returnString += $"{Environment.NewLine}networkObjectBatchSend = {cfg.networkObjectBatchSend.ToString()};";
                returnString += $"{Environment.NewLine}networkObjectBatchCompute = {cfg.networkObjectBatchCompute.ToString()};";
                returnString += $"{Environment.NewLine}defaultVisibility = {cfg.defaultVisibility.ToString()};";
                returnString += $"{Environment.NewLine}defaultObjectViewDistance = {cfg.defaultObjectViewDistance.ToString()};";
                returnString += $"{Environment.NewLine}disableBaseDamage = {cfg.disableBaseDamage.ToString()};";
                returnString += $"{Environment.NewLine}disableContainerDamage = {cfg.disableContainerDamage.ToString()};";
                returnString += $"{Environment.NewLine}disableRespawnDialog = {cfg.disableRespawnDialog.ToString()};";
                returnString += $"{Environment.NewLine}pingWarning = {cfg.pingWarning.ToString()};";
                returnString += $"{Environment.NewLine}pingCritical = {cfg.pingCritical.ToString()};";
                returnString += $"{Environment.NewLine}MaxPing = {cfg.MaxPing.ToString()};";
                returnString += $"{Environment.NewLine}serverFpsWarning = {cfg.serverFpsWarning.ToString()};";
                returnString += $"{Environment.NewLine}motdInterval = {cfg.motdInterval.ToString()};";

                returnString += $"{Environment.NewLine}motd[] = {{";

                string motdLines = "";

                if (cfg.motd != null)
                {
                    foreach (string line in cfg.motd)
                    {
                        if (line != null)
                        {
                            motdLines += $"\"{line}\",";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(motdLines))
                {
                    returnString += motdLines.Remove(motdLines.Length - 1);
                }

                returnString += "};";

                returnString += Environment.NewLine;
                returnString += Environment.NewLine;
                returnString += $"{Environment.NewLine}class Missions";
                returnString += $"{Environment.NewLine}{{";
                returnString += $"{Environment.NewLine}    class DayZ";
                returnString += $"{Environment.NewLine}    {{";
                returnString += $"{Environment.NewLine}        template = \"{cfg.template}\";";
                returnString += $"{Environment.NewLine}    }};";
                returnString += $"{Environment.NewLine}}};";
            }

            return returnString;
        }
    }
}
