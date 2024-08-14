import "./ManagerConfigEditor.css"
import { useEffect, useState } from 'react';
import { TextField, Box, Button } from '@mui/material';

interface ManagerConfig {
    steamUsername: string;
    steamPassword: string;
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
    missionTemplateName: string;
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

    const handleClientModChange = (event: React.ChangeEvent<HTMLInputElement>, i: number ) => {
        if (!(managerConfig === undefined)) {
            let changedMod: Mod = managerConfig.clientMods[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    clientMods: [
                        ...managerConfig.clientMods.filter((_, index) => index < i),
                        {
                            ...changedMod,
                            [event.target.id]: event.target.value
                        },
                        ...managerConfig.clientMods.filter((_, index) => index > i)
                    ]
                }
            );
        }
    }

    const deleteClientMod = (i: number) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    clientMods: [
                        ...managerConfig.clientMods.filter((_, index) => index !== i)
                    ]
                }
            )
        }
    }

    const handleServerModChange = (event: React.ChangeEvent<HTMLInputElement>, i: number) => {
        if (!(managerConfig === undefined)) {
            let changedMod: Mod = managerConfig.serverMods[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    serverMods: [
                        ...managerConfig.serverMods.filter((_, index) => index < i),
                        {
                            ...changedMod,
                            [event.target.id]: event.target.value
                        },
                        ...managerConfig.serverMods.filter((_, index) => index > i),
                    ]
                }
            );
        }
    }

    const deleteServerMod = (i: number) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    serverMods: [
                        ...managerConfig.serverMods.filter((_, index) => index !== i)
                    ]
                }
            )
        }
    }

    let texts: JSX.Element[] = new Array;
    if (!(managerConfig === undefined)) {
        Object.entries(managerConfig).map(([key, value]) => (key != "serverMods" && key != "clientMods") && texts.push(<TextField key={key} id={key} variant="outlined" label={key} defaultValue={value} onChange={handleChange} />));
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
                {
                    texts.map(x => { return x; })
                }
            </div>
            <h3>Client Mods</h3>
            <div>
                {
                    managerConfig.clientMods.map((mod, index) => {
                        return (
                            <div id={"ClientMod" + index}>
                                <TextField id="workshopID" variant="outlined" label="Workshop ID" defaultValue={mod.workshopID} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleClientModChange(event, index)} />
                                <TextField id="name" variant="outlined" label="Name" defaultValue={mod.name} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleClientModChange(event, index)} />
                                <Button id="deleteButton" onClick={() => deleteClientMod(index)}>
                                    Delete
                                </Button>
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
                    managerConfig.serverMods.map((mod, index) => {
                        return (
                            <div id={"ServerMod" + index}>
                                <TextField id="workshopID" variant="outlined" label="Workshop ID" defaultValue={mod.workshopID} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleServerModChange(event, index)} />
                                <TextField id="name" variant="outlined" label="Name" defaultValue={mod.name} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleServerModChange(event, index)} />
                                <Button id="deleteButton" onClick={() => deleteServerMod(index)}>
                                    Delete
                                </Button>
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
            <h1 id="tableLabel">Manager Configurations</h1>
            <div>
                <Button
                    onClick={postManagerConfig}
                >
                    Save Config!
                </Button>
            </div>
            <div>
                {contents}
            </div>
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