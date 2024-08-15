import { useEffect, useState } from "react";
import { Box, Button, TextField } from '@mui/material';



interface ServerConfig {
    hostname: string;
    password: string;
    passwordAdmin: string;
    enableWhitelist: number;
    disableBanlist: boolean;
    disablePrioritylist: boolean;
    maxPlayers: number;
    verifySignatures: number;
    forceSameBuild: number;
    disableVoN: number;
    vonCodecQuality: number;
    enableCfgGameplayFile: number;
    disable3rdPerson: number;
    disableCrosshair: number;
    disablePersonalLight: number;
    lightingConfig: number;
    serverTime: string;
    serverTimeAcceleration: number;
    serverNightTimeAcceleration: number;
    serverTimePersistent: number;
    guaranteedUpdates: number;
    loginQueueConcurrentPlayers: number;
    loginQueueMaxPlayers: number;
    instanceId: number;
    storageAutoFix: number;
    steamQueryPort: number;
    respawnTime: number;
    timeStampFormat: string;
    logAverageFps: number;
    logMemory: number;
    logPlayers: number;
    logFile: string;
    adminLogPlayerHitsOnly: number;
    adminLogPlacement: number;
    adminLogBuildActions: number;
    adminLogPlayerList: number;
    disableMultiAccountMitigation: boolean;
    enableDebugMonitor: number;
    allowFilePatching: number;
    simulatedPlayersBatch: number;
    multithreadedReplication: number;
    speedhackDetection: number;
    networkRangeClose: number;
    networkRangeNear: number;
    networkRangeFar: number;
    networkRangeDistantEffect: number;
    networkObjectBatchSend: number;
    networkObjectBatchCompute: number;
    defaultVisibility: number;
    defaultObjectViewDistance: number;
    disableBaseDamage: number;
    disableContainerDamage: number;
    disableRespawnDialog: number;
    pingWarning: number;
    pingCritical: number;
    MaxPing: number;
    serverFpsWarning: number;
    motdInterval: number;
    motd: string[];
}

export default function ServerConfigEditor() {

    const [serverConfig, setServerConfig] = useState<ServerConfig>();

    useEffect(() => {
        populateServerConfig();
    }, []);
    
    const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (!(serverConfig === undefined)) {
            setServerConfig(
                {
                    ...serverConfig,
                    [event.target.id]: event.target.value
                }
            );
        }
    }

    const handleMOTDLineChange = (event: React.ChangeEvent<HTMLInputElement>, i: number) => {
        if (!(serverConfig === undefined)) {
            let changedLine: string = serverConfig.motd[i];
            if (!(changedLine === undefined)) {
                setServerConfig(
                    {
                        ...serverConfig,
                        motd: [
                            ...serverConfig.motd.filter((_, index) => index < i),
                            event.target.value,
                            ...serverConfig.motd.filter((_, index) => index > i),
                        ]
                    }
                );
            }
        }
    }

    const createMOTDLine = (line: string) => {
        if (!(serverConfig === undefined)) {
            setServerConfig(
                {
                    ...serverConfig,
                    motd: [
                        ...serverConfig.motd,
                        line
                    ]
                }
            );
        }
    }

    const deleteMOTDLine = (i: number) => {
        if (!(serverConfig === undefined)) {
            setServerConfig(
                {
                    ...serverConfig,
                    motd: [
                        ...serverConfig.motd.filter((_, index) => index !== i)
                    ]
                }
            )
        }
    }

    let texts: JSX.Element[] = new Array;
    if (!(serverConfig === undefined)) {
        Object.entries(serverConfig).map(([key, value]) => (key != "motd" && key != "template") && texts.push(<TextField key={key} id={key} variant="outlined" label={key} defaultValue={value} onChange={handleChange} />));
    }

    const contents = serverConfig === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <Box
            component="form"
            sx={{
                '& .MuiTextField-root': { m: 1, width: '25ch' },
            }}
            noValidate
            autoComplete="off"
        >
            <div>
                {
                    texts.map(x => { return x; })
                }
            </div>
            <h3>Motto of the day</h3>
            <div>
                {
                    serverConfig.motd.map((line, index) => {
                        return (
                            <div key={index}>
                                <TextField id={"line" + index} variant="outlined" label={"Line " + (index + 1)} defaultValue={line} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleMOTDLineChange(event, index)} />
                                <Button id="deleteButton" onClick={() => deleteMOTDLine(index)}>
                                    Delete
                                </Button>
                            </div>
                        )
                    })
                }
                <Button
                    onClick={() => { createMOTDLine("newLine") }}
                >
                    Add new Line
                </Button>
            </div>
        </Box>

    return (
        <div>
            <h1 id="tableLabel">Server Configurations</h1>
            <div>
                <Button
                    onClick={postServerConfig}
                >
                    Save Config!
                </Button>
                <Button
                    onClick={saveServerConfig}
                >
                    Manually save Server Config to files!
                </Button>
            </div>
            <div>
                {contents}
            </div>
        </div>
    )

    async function populateServerConfig() {
        const response = await fetch('ServerConfig/GetServerConfig');
        const result = (await response.json()) as ServerConfig;
        setServerConfig(result);
    }

    async function postServerConfig() {
        const response = await fetch('ServerConfig/PostServerConfig', {
            method: "POST",
            mode: "cors",
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(serverConfig)
        });
        const result = await response.text()
        alert(result);
    }

    async function saveServerConfig() {
        const response = await fetch('ServerConfig/SaveServerConfig');
        const result = await response.text()
        alert(result);
    }


}