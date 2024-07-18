import "./ManagerConfigEditor.css"
import { useEffect, useState } from 'react';
import { TextField, Box, Button } from '@mui/material';

interface ManagerConfig {
    steamUsername: string;
    steamPassword: string;
    serverPath: string;
    steamCMDPath: string;
    becPath: string;
    workshopPath: string;
    backupPath: string;
    missionName: string;
    instanceId: number;
    serverConfigName: string;
    profileName: string;
    port: number;
    steamQueryPort: number;
    RConPort: number;
    cpuCount: number;
    noFilePatching: boolean;
    doLogs: boolean;
    adminLog: boolean;
    freezeCheck: boolean;
    netLog: boolean;
    limitFPS: number;
    vanillaMissionName: string;
    missionTemplatePath: string;
    expansionDownloadPath: string;
    mapName: string;
    restartOnUpdate: boolean;
    restartInterval: number;
    autoStartServer: boolean;
    clientMods: Mod[];
    serverMods: Mod[];
}

interface Mod {
    workshopID: number;
    name: string;
}

export default function ManagerConfigEditor() {
    const [managerConfig, setManagerConfig] = useState<ManagerConfig>();

    useEffect(() => {
        populateManagerConfig();
    }, []);

    const createClientMod = (mod: Mod) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    clientMods: [
                        ...managerConfig.clientMods,
                        mod
                    ]
                }
            );
        }
    }

    const createServerMod = (mod: Mod) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    serverMods: [
                        ...managerConfig.serverMods,
                        mod
                    ]
                }
            );
        }
    }

    const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    [event.target.id]: event.target.value
                }
            );
        }
    }

    const handleClientModChange = (event: React.ChangeEvent<HTMLInputElement>, workshopID: number, name: string) => {
        if (!(managerConfig === undefined)) {
            let changedMod: Mod | undefined = managerConfig.clientMods.find(mod => mod.workshopID === workshopID || mod.name === name);
            if (!(changedMod === undefined)) {
                setManagerConfig(
                    {
                        ...managerConfig,
                        clientMods: [
                            ...managerConfig.clientMods.filter(mod => !(mod == changedMod)),
                            {
                                ...changedMod,
                                [event.target.id]: event.target.value
                            }
                        ]
                    }
                );
            }
        }
    }

    const handleServerModChange = (event: React.ChangeEvent<HTMLInputElement>, workshopID: number, name: string) => {
        if (!(managerConfig === undefined)) {
            let changedMod: Mod | undefined = managerConfig.serverMods.find(mod => mod.workshopID === workshopID || mod.name === name);
            if (!(changedMod === undefined)) {
                setManagerConfig(
                    {
                        ...managerConfig,
                        serverMods: [
                            ...managerConfig.serverMods.filter(mod => !(mod == changedMod)),
                            {
                                ...changedMod,
                                [event.target.id]: event.target.value
                            }
                        ]
                    }
                );
            }
        }
    }

    const contents = managerConfig === undefined
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
                <TextField id="steamUsername" variant="outlined" label="Steam Username" defaultValue={managerConfig.steamUsername} onChange={handleChange } />
                <TextField id="steamPassword" variant="outlined" label="Steam Password" defaultValue={managerConfig.steamPassword} onChange={handleChange} />
                <TextField id="serverPath" variant="outlined" label="Server Path" defaultValue={managerConfig.serverPath} onChange={handleChange} />
                <TextField id="steamCMDPath" variant="outlined" label="Steam CMD Path" defaultValue={managerConfig.steamCMDPath} onChange={handleChange} />
                <TextField id="becPath" variant="outlined" label="BEC Path" defaultValue={managerConfig.becPath} onChange={handleChange} />
                <TextField id="workshopPath" variant="outlined" label="Workshop Path" defaultValue={managerConfig.workshopPath} onChange={handleChange} />
                <TextField id="backupPath" variant="outlined" label="Backup Path" defaultValue={managerConfig.backupPath} onChange={handleChange} />
                <TextField id="missionName" variant="outlined" label="Mission Name" defaultValue={managerConfig.missionName} onChange={handleChange} />
                <TextField id="instanceId" variant="outlined" label="Instance ID" defaultValue={managerConfig.instanceId} onChange={handleChange} />
                <TextField id="serverConfigName" variant="outlined" label="Server Config Name" defaultValue={managerConfig.serverConfigName} onChange={handleChange} />
                <TextField id="profileName" variant="outlined" label="Profile Name" defaultValue={managerConfig.profileName} onChange={handleChange} />
                <TextField id="port" variant="outlined" label="Port" defaultValue={managerConfig.port} onChange={handleChange} />
                <TextField id="steamQueryPort" variant="outlined" label="Steam Query Port" defaultValue={managerConfig.steamQueryPort} onChange={handleChange} />
                <TextField id="RConPort" variant="outlined" label="RCon Port" defaultValue={managerConfig.RConPort} onChange={handleChange} />
                <TextField id="cpuCount" variant="outlined" label="CPU Count" defaultValue={managerConfig.cpuCount} onChange={handleChange} />
                <TextField id="noFilePatching" variant="outlined" label="No File Patching" defaultValue={managerConfig.noFilePatching} onChange={handleChange} />
                <TextField id="doLogs" variant="outlined" label="Do Logs" defaultValue={managerConfig.doLogs} onChange={handleChange} />
                <TextField id="adminLog" variant="outlined" label="Admin Log" defaultValue={managerConfig.adminLog} onChange={handleChange} />
                <TextField id="freezeCheck" variant="outlined" label="Freeze Check" defaultValue={managerConfig.freezeCheck} onChange={handleChange} />
                <TextField id="netLog" variant="outlined" label="Net Log" defaultValue={managerConfig.netLog} onChange={handleChange} />
                <TextField id="limitFPS" variant="outlined" label="Limit FPS" defaultValue={managerConfig.limitFPS} onChange={handleChange} />
                <TextField id="vanillaMissionName" variant="outlined" label="Vanilla Mission Name" defaultValue={managerConfig.vanillaMissionName} onChange={handleChange} />
                <TextField id="missionTemplatePath" variant="outlined" label="Mission Template Path" defaultValue={managerConfig.missionTemplatePath} onChange={handleChange} />
                <TextField id="expansionDownloadPath" variant="outlined" label="Expansion Download Path" defaultValue={managerConfig.expansionDownloadPath} onChange={handleChange} />
                <TextField id="mapName" variant="outlined" label="mapName" defaultValue={managerConfig.mapName} onChange={handleChange} />
                <TextField id="restartOnUpdate" variant="outlined" label="Restart On Update" defaultValue={managerConfig.restartOnUpdate} onChange={handleChange} />
                <TextField id="restartInterval" variant="outlined" label="Restart Interval" defaultValue={managerConfig.restartInterval} onChange={handleChange} />
                <TextField id="autoStartServer" variant="outlined" label="Autostart Server" defaultValue={managerConfig.autoStartServer} onChange={handleChange} />
            </div>
            <h3>Client Mods</h3>
            <div>
                {
                    managerConfig.clientMods.map((mod) => {
                        return (
                            <div id={mod.workshopID.toString()}>
                                <TextField id="WorkshopID" variant="outlined" label="Workshop ID" defaultValue={mod.workshopID} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleClientModChange(event, mod.workshopID, mod.name)} />
                                <TextField id="Name" variant="outlined" label="Name" defaultValue={mod.name} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleClientModChange(event, mod.workshopID, mod.name)} />
                            </div>
                        )
                    })
                }
                <Button onClick={() => { createClientMod({ workshopID: 0, name: "" }) }}>
                    Add new Row
                </Button>
            </div>
            <h3>Server Mods</h3>
            <div>
                {
                    managerConfig.serverMods.map((mod) => {
                        return (
                            <div id={mod.workshopID.toString()}>
                                <TextField id="WorkshopID" variant="outlined" label="Workshop ID" defaultValue={mod.workshopID} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleServerModChange(event, mod.workshopID, mod.name)} />
                                <TextField id="Name" variant="outlined" label="Name" defaultValue={mod.name} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleServerModChange(event, mod.workshopID, mod.name)} />
                            </div>
                        )
                    })
                }
                <Button
                    onClick={() => { createServerMod({ workshopID: 0, name: "" }) }}
                >
                    Add new Row
                </Button>
            </div>
        </Box>

    return (
        <div>
            <h1 id="tableLabel">Manager Configs</h1>
            <div>
                <Button
                    onClick={postManagerConfig}
                >
                    Save Config!
                </Button>
            </div>
            <p>
                {contents}
            </p>
        </div>
    );

    async function populateManagerConfig() {
        const response = await fetch('ManagerConfig/GetManagerConfig');
        const result = (await response.json()) as ManagerConfig;
        setManagerConfig(result);
    }

    async function postManagerConfig() {
        const response = await fetch('ManagerConfig/PostManagerConfig', {
            method: "POST",
            mode: "cors",
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(managerConfig)
        });
        const result = await response.text()
        alert(result);
    }
}