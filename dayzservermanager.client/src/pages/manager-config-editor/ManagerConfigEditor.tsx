import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import { Box, Button, TextField } from '@mui/material';
import { useEffect, useState } from 'react';
import { Dict } from "styled-components/dist/types";
import "./ManagerConfigEditor.css";

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
    customMessages: CustomMessage[];
}

interface Mod {
    id: number,
    workshopID: number;
    name: string;
}

interface CustomMessage {
    id: number;
    isTimeOfDay: boolean,
    waitTime: Dict,
    interval: Dict,
    title: string,
    message: string,
    icon: string,
    color: string
}

export default function ManagerConfigEditor() {
    const [managerConfig, setManagerConfig] = useState<ManagerConfig>();

    useEffect(() => {
        populateManagerConfig();
    }, []);

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

    const findFirstAvailableId = (modList: Mod[]) => {
        let nextId = 0;
        let modIds: number[] = new Array<number>();
        modList.map((mod) => { modIds.push(mod.id) });

        for (let i: number = 0; i < modList.length; i++) {
            if (modIds.find(id => id == i) === undefined) {
                return i;
            }
            else {
                nextId = i + 1;
            }
        }
        return nextId;
    }

    const findFirstAvailableCustomMessagesId = () => {
        let nextId = 0;
        if (managerConfig !== undefined) {
            let modIds: number[] = new Array<number>();
            managerConfig.customMessages.map((mod) => { modIds.push(mod.id) });

            for (let i: number = 0; i < managerConfig.customMessages.length; i++) {
                if (modIds.find(id => id == i) === undefined) {
                    return i;
                }
                else {
                    nextId = i + 1;
                }
            }
        }
        return nextId;
    }

    const createClientMod = () => {
        if (!(managerConfig === undefined)) {
            let newId = findFirstAvailableId(managerConfig.clientMods);
            setManagerConfig(
                {
                    ...managerConfig,
                    clientMods: [
                        ...managerConfig.clientMods,
                        { id: newId, workshopID: 0, name: ""}
                    ]
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

    const deleteClientMod = (id: number) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    clientMods: [
                        ...managerConfig.clientMods.filter((mod) => mod.id !== id)
                    ]
                }
            );
        }
    }

    const moveClientModUp = (i: number) => {
        if (!(managerConfig === undefined)) {
            let movedMod = managerConfig.clientMods[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    clientMods: [
                        ...managerConfig.clientMods.filter((_, index) => index < i - 1),
                        movedMod,
                        ...managerConfig.clientMods.filter((_, index) => (index > i - 2 && index !== i))
                    ]
                }
            );
        }
    }

    const moveClientModDown = (i: number) => {
        if (!(managerConfig === undefined)) {
            let movedMod = managerConfig.clientMods[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    clientMods: [
                        ...managerConfig.clientMods.filter((_, index) => (index < i + 2 && index !== i)),
                        movedMod,
                        ...managerConfig.clientMods.filter((_, index) => index > i + 1)
                    ]
                }
            );

        }
    }

    const createServerMod = () => {
        if (!(managerConfig === undefined)) {
            let newId = findFirstAvailableId(managerConfig.clientMods);
            setManagerConfig(
                {
                    ...managerConfig,
                    serverMods: [
                        ...managerConfig.serverMods,
                        {id:newId,workshopID:0,name:""}
                    ]
                }
            );
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

    const deleteServerMod = (id: number) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    serverMods: [
                        ...managerConfig.serverMods.filter((mod) => mod.id !== id)
                    ]
                }
            );
        }
    }

    const moveServerModUp = (i: number) => {
        if (!(managerConfig === undefined)) {
            let movedMod = managerConfig.serverMods[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    serverMods: [
                        ...managerConfig.serverMods.filter((_, index) => index < i - 1),
                        movedMod,
                        ...managerConfig.serverMods.filter((_, index) => index > i - 2 && index !== i)
                    ]

                }
            );
        }
    }

    const moveServerModDown = (i: number) => {
        if (!(managerConfig === undefined)) {
            let movedMod = managerConfig.serverMods[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    serverMods: [
                        ...managerConfig.serverMods.filter((_, index) => index < i + 2 && index !== i),
                        movedMod,
                        ...managerConfig.serverMods.filter((_, index) => index > i + 1)
                    ]

                }
            );
        }
    }

    const createCustomMessage = () => {
        if (!(managerConfig === undefined)) {
            let newId = findFirstAvailableCustomMessagesId();
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: [
                        ...managerConfig.customMessages,
                        {
                            id: newId, isTimeOfDay: false, waitTime: { hours: 0, minutes: 0, seconds: 0 }, interval: { hours: 0, minutes: 0, seconds: 0 }, title: "", message: "", icon: "", color: ""
                        } 
                    ]
                }
            );
        }
    }

    const handleCustomMessagesChange = (event: React.ChangeEvent<HTMLInputElement>, i: number) => {
        if (!(managerConfig === undefined)) {
            let changedMessage: CustomMessage = managerConfig.customMessages[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: [
                        ...managerConfig.customMessages.filter((_, index) => index < i),
                        {
                            ...changedMessage,
                            [event.target.id]: event.target.value
                        },
                        ...managerConfig.customMessages.filter((_, index) => index > i)
                    ]
                }
            )
        }
    }

    const handleWaitTimeChange = (event: React.ChangeEvent<HTMLInputElement>, i: number) => {
        if (!(managerConfig === undefined)) {
            let changedMessage: CustomMessage = managerConfig.customMessages[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: [
                        ...managerConfig.customMessages.filter((_, index) => index < i),
                        {
                            ...changedMessage,
                            waitTime: {
                                ...changedMessage.waitTime,
                                [event.target.id]: event.target.value
                            }
                        },
                        ...managerConfig.customMessages.filter((_, index) => index > i)
                    ]
                }
            )
        }
    }

    const handleIntervalChange = (event: React.ChangeEvent<HTMLInputElement>, i: number) => {
        if (!(managerConfig === undefined)) {
            let changedMessage: CustomMessage = managerConfig.customMessages[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: [
                        ...managerConfig.customMessages.filter((_, index) => index < i),
                        {
                            ...changedMessage,
                            interval: {
                                ...changedMessage.interval,
                                [event.target.id]: event.target.value
                            }
                        },
                        ...managerConfig.customMessages.filter((_, index) => index > i)
                    ]
                }
            )
        }
    }

    const deleteCustomMessage = (id: number) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: [
                        ...managerConfig.customMessages.filter((message) => message.id !== id)
                    ]
                }
            )
        }
    }

    const moveMessageUp = (i: number) => {
        if (!(managerConfig === undefined)) {
            let movedMessage = managerConfig.customMessages[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: [
                        ...managerConfig.customMessages.filter((_, index) => index < i - 1),
                        movedMessage,
                        ...managerConfig.customMessages.filter((_, index) => index > i - 2 && index !== i)
                    ]

                }
            );
        }
    }

    const moveMessageDown = (i: number) => {
        if (!(managerConfig === undefined)) {
            let movedMessage = managerConfig.customMessages[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: [
                        ...managerConfig.customMessages.filter((_, index) => index < i + 2 && index !== i),
                        movedMessage,
                        ...managerConfig.customMessages.filter((_, index) => index > i + 1)
                    ]

                }
            );
        }
    }

    let texts: JSX.Element[] = new Array;
    if (managerConfig != null) {
        Object.entries(managerConfig).map(([key, value]) => (key != "serverMods" && key != "clientMods" && key != "customMessages") && texts.push(<TextField key={key} id={key} variant="outlined" label={key} defaultValue={value} onChange={handleChange} />));
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
                            <div key={mod.id} id={"ClientMod" + index}>
                                <Button onClick={() => { moveClientModUp(index) }}><KeyboardArrowUpIcon/></Button>
                                <Button onClick={() => { moveClientModDown(index) }}><KeyboardArrowDownIcon/></Button>
                                <TextField id="workshopID" variant="outlined" label="Workshop ID" defaultValue={mod.workshopID} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleClientModChange(event, index)} />
                                <TextField fullWidth id="name" variant="outlined" label="Name" defaultValue={mod.name} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleClientModChange(event, index)} />
                                <Button id="deleteButton" onClick={() => deleteClientMod(mod.id)}>
                                    Delete
                                </Button>
                            </div>
                        )
                    })
                }
                <Button onClick={() => { createClientMod() }}>
                    Add new Row
                </Button>
            </div>
            <h3>Server Mods</h3>
            <div>
                {
                    managerConfig.serverMods.map((mod, index) => {
                        return (
                            <div key={mod.id} id={"ServerMod" + index}>
                                <Button onClick={() => { moveServerModUp(index) }}><KeyboardArrowUpIcon/></Button>
                                <Button onClick={() => { moveServerModDown(index) }}><KeyboardArrowDownIcon /></Button>
                                <TextField id="workshopID" variant="outlined" label="Workshop ID" defaultValue={mod.workshopID} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleServerModChange(event, index)} />
                                <TextField fullWidth id="name" variant="outlined" label="Name" defaultValue={mod.name} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleServerModChange(event, index)} />
                                <Button id="deleteButton" onClick={() => deleteServerMod(mod.id)}>
                                    Delete
                                </Button>
                            </div>
                        )
                    })
                }
                <Button
                    onClick={() => { createServerMod() }}
                >
                    Add new Row
                </Button>
            </div>
            <h3>Custom Messages</h3>
            <div>
                {
                    managerConfig.customMessages.map((message, index) => {
                        return (
                            <div className="messageContainer" key={message.id} id={"Message" + index}>
                                <Button onClick={() => { moveMessageUp(index) }}><KeyboardArrowUpIcon /></Button>
                                <Button onClick={() => { moveMessageDown(index) }}><KeyboardArrowDownIcon /></Button>
                                <div className="subContainer">
                                    <h4>Wait Time</h4>
                                    <TextField id="hours" variant="outlined" label="Hours" defaultValue={message.waitTime["hours"]} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleWaitTimeChange(event, index)} />
                                    <TextField id="minutes" variant="outlined" label="Minutes" defaultValue={message.waitTime["minutes"]} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleCustomMessagesChange(event, index)} />
                                    <TextField id="seconds" variant="outlined" label="Seconds" defaultValue={message.waitTime["seconds"]} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleCustomMessagesChange(event, index)} />
                                </div>
                                <div className="subContainer">
                                    <h4>Interval</h4>
                                    <TextField id="hours" variant="outlined" label="Hours" defaultValue={message.interval["hours"]} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleIntervalChange(event, index)} />
                                    <TextField id="minutes" variant="outlined" label="Minutes" defaultValue={message.interval["minutes"]} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleIntervalChange(event, index)} />
                                    <TextField id="seconds" variant="outlined" label="Seconds" defaultValue={message.interval["seconds"]} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleIntervalChange(event, index)} />
                                </div>
                                <div className="subContainer">
                                    <TextField id="Title" variant="outlined" label="Title" defaultValue={message.title} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleCustomMessagesChange(event, index)} />
                                    <TextField fullWidth multiline={true} id="Message" variant="outlined" label="Message" defaultValue={message.message} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleCustomMessagesChange(event, index)} />
                                    <TextField id="Icon" variant="outlined" label="Icon" defaultValue={message.icon} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleCustomMessagesChange(event, index)} />
                                    <TextField id="Color" variant="outlined" label="Color" defaultValue={message.color} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleCustomMessagesChange(event, index)} />
                                    <TextField id="IsTimeOfDay" variant="outlined" label="Is Time Of Day" defaultValue={message.isTimeOfDay} onChange={(event: React.ChangeEvent<HTMLInputElement>) => handleCustomMessagesChange(event, index)} />
                                </div>
                                <Button id="deleteButton" onClick={() => deleteCustomMessage(message.id)}>
                                    Delete
                                </Button>
                            </div>
                        )
                    })
                }
                <Button
                    onClick={() => { createCustomMessage() }}
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
        let json = JSON.stringify(managerConfig);
        const response = await fetch('ManagerConfig/PostManagerConfig', {
            method: "POST",
            mode: "cors",
            headers: { 'Content-Type': 'application/json' },
            body: json
        });
        const result = await response.text()
        alert(result);
    }
}