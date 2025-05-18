
import { CheckboxVisibility, DefaultButton, DetailsList, DetailsRow, getTheme, IColumn, IDetailsRowProps, IDragDropContext, IDragDropEvents, initializeIcons, IObjectWithKey, mergeStyles, Selection, SelectionMode, TextField } from '@fluentui/react';
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import { JSX, useEffect, useMemo, useState } from 'react';
import { Dict } from "styled-components/dist/types";
import ReloadButton from '../../common/components/reload-button/ReloadButton';
import SaveButton from '../../common/components/save-button/SaveButton';
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
    const [draggedClientMods, setDraggedClientMods] = useState<Mod>();
    const [draggedServerMods, setDraggedServerMods] = useState<Mod>();
    const [draggedClientModsIndex, setDraggedClientModsIndex] = useState<number>(-1);
    const [draggedServerModsIndex, setDraggedServerModsIndex] = useState<number>(-1);
    const [, setSelectedClientMods] = useState<IObjectWithKey[]>();
    const [, setSelectedServerMods] = useState<IObjectWithKey[]>();

    initializeIcons();

    useEffect(() => {
        handleLoad();
    }, []);

    const handleSave = () => {
        PostManagerConfig('ManagerConfig/PostManagerConfig', JSON.stringify(managerConfig));
    }

    const clientModsSelection: Selection = useMemo(() => new Selection(
        {
            onSelectionChanged: () => {
                setSelectedClientMods(clientModsSelection.getSelection());
            },
            selectionMode: SelectionMode.multiple,
            getKey: (item) => (item as Mod).id
        }),
        []
    );

    const serverModsSelection: Selection = useMemo(() => new Selection(
        {
            onSelectionChanged: () => {
                setSelectedServerMods(serverModsSelection.getSelection());
            },
            selectionMode: SelectionMode.multiple,
            getKey: (item) => (item as Mod).id
        }),
        []
    );

    const handleLoad = () => {
        PopulateManagerConfig(setManagerConfig, 'ManagerConfig/GetManagerConfig');
    };

    const handleChange = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue: string | undefined) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    [event.currentTarget.id]: (newValue && newValue.toLowerCase() == "true") ? true : (newValue && newValue.toLowerCase() == "false" ? false : newValue)
                }
            );
        }
    }

    const findFirstAvailableId = (modList: Mod[]) => {
        let nextId = 0;
        let modIds: number[] = new Array<number>();
        modList.map((mod) => { modIds.push(mod.id) });

        for (let i: number = 0; i < modIds.length; i++) {
            if (modIds.find(id => id === i) === undefined) {
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

    const handleClientModBlur = (event: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement, Element>, mod: Mod, key: string) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    clientMods: managerConfig.clientMods.map((oldMod) => oldMod.id === mod.id ? {...mod, [key]: event.target.value} : oldMod)
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

    const createServerMod = () => {
        if (!(managerConfig === undefined)) {
            let newId = findFirstAvailableId(managerConfig.serverMods);
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

    const handleServerModBlur = (event: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement, Element>, mod: Mod, key: string) => {
        if (!(managerConfig === undefined)) {
            setManagerConfig(
                {
                    ...managerConfig,
                    serverMods: managerConfig.serverMods.map((oldMod) => oldMod.id === mod.id ? { ...mod, [key]: key === "workshopID" ? parseInt(event.target.value) : event.target.value } : oldMod)
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

    const handleCustomMessagesChange = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, i: number, newValue: string | undefined) => {
        if (!(managerConfig === undefined)) {
            let changedMessage: CustomMessage = managerConfig.customMessages[i];
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: [
                        ...managerConfig.customMessages.filter((_, index) => index < i),
                        {
                            ...changedMessage,
                            [event.currentTarget.id]: newValue
                        },
                        ...managerConfig.customMessages.filter((_, index) => index > i)
                    ]
                }
            )
        }
    }

    const handleWaitTimeChange = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, i: number, newValue: string | undefined) => {
        if (managerConfig) {
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
                                [event.currentTarget.id]: newValue
                            }
                        },
                        ...managerConfig.customMessages.filter((_, index) => index > i)
                    ]
                }
            )
        }
    }

    const handleIntervalChange = (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, i: number, newValue: string | undefined) => {
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
                                [event.currentTarget.id]: newValue
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

    const insertBeforeClientMods = (item: Mod): void => {
        if (managerConfig) {
            const draggedItems = clientModsSelection.isIndexSelected(draggedClientModsIndex)
                ? (clientModsSelection.getSelection() as Mod[])
                : [draggedClientMods!];

            const items = managerConfig.clientMods.filter(itm => draggedItems.indexOf(itm) === -1);

            const insertIndex = managerConfig.clientMods.indexOf(item as Mod);
            items.splice(insertIndex, 0, ...draggedItems);

            setManagerConfig({
                ...managerConfig,
                clientMods: items
            });
        }
    };

    const insertBeforeServerMods = (item: Mod): void => {
        if (managerConfig) {
            const draggedItems = serverModsSelection.isIndexSelected(draggedServerModsIndex)
                ? (serverModsSelection.getSelection() as Mod[])
                : [draggedServerMods!];

            const items = managerConfig.serverMods.filter(itm => draggedItems.indexOf(itm) === -1);

            const insertIndex = managerConfig.serverMods.indexOf(item as Mod);
            items.splice(insertIndex, 0, ...draggedItems);

            setManagerConfig({
                ...managerConfig,
                serverMods: items
            });
        }
    };

    const handleClientModsDragStart = (item: any, itemIndex: number) => {
        setDraggedClientMods(item);
        setDraggedClientModsIndex(itemIndex!);
    };

    const handleServerModsDragStart = (item: any, itemIndex: number) => {
        setDraggedServerMods(item);
        setDraggedServerModsIndex(itemIndex!);
    };

    const handleClientModsDrop = (item: any) => {
        if (draggedClientMods) {
            insertBeforeClientMods(item);
        }
    };

    const handleServerModsDrop = (item: any) => {
        if (draggedServerMods) {
            insertBeforeServerMods(item);
        }
    };

    const handleClientModsDragEnd = () => {
        setDraggedClientMods(undefined);
        setDraggedClientModsIndex(-1);
    };

    const handleServerModsDragEnd = () => {
        setDraggedServerMods(undefined);
        setDraggedServerModsIndex(-1);
    };

    const handleDragEnter = (item: any) => {
        console.log("Drag entered:", item);
        // return string is the css classes that will be added to the entering element.
        return mergeStyles({
            backgroundColor: getTheme().palette.neutralLight,
        });
    };

    const handleDragLeave = () => {
        return;
    };

    const dragDropEvents: IDragDropEvents = {
        canDrop: (_dropContext?: IDragDropContext, _dragContext?: IDragDropContext) => {
            return true;
        },
        canDrag: (_item?: any) => {
            return true;
        }
    };

    const clientModsColumns: IColumn[] = [
        {
            key: "name",
            name: "Name",
            fieldName: "name",
            minWidth: 160,
            onRender: (mod: Mod) => {
                return (
                    <TextField id={"name-" + mod.id} defaultValue={mod.name} onBlur={(event) => handleClientModBlur(event, mod, "name")} />
                )
            }
        },
        {
            key: "workshopID",
            name: "Workshop ID",
            fieldName: "workshopID",
            minWidth: 160,
            onRender: (mod: Mod) => {
                return (
                    <TextField id={"workshopID-" + mod.id} defaultValue={mod.workshopID.toString()} onBlur={(event) => handleClientModBlur(event, mod, "workshopID")} />
                )
            }
        },
        {
            key: "delete",
            name: "Delete",
            fieldName: "delete",
            minWidth: 80,
            onRender: (mod: Mod) => {
                return (
                    <DefaultButton
                        onClick={() => deleteClientMod(mod.id)}
                        className="Button"
                    >
                        <DeleteIcon />
                    </DefaultButton>
                )
            }
        }
    ]

    const serverModsColumns: IColumn[] = [
        {
            key: "name",
            name: "Name",
            fieldName: "name",
            minWidth: 160,
            onRender: (mod: Mod) => {
                return (
                    <TextField id={"name-" + mod.id} defaultValue={mod.name} onBlur={(event) => handleServerModBlur(event, mod, "name")} />
                )
            }
        },
        {
            key: "workshopID",
            name: "Workshop ID",
            fieldName: "workshopID",
            minWidth: 160,
            onRender: (mod: Mod) => {
                return (
                    <TextField id={"workshopID-" + mod.id} defaultValue={mod.workshopID.toString()} onBlur={(event) => handleServerModBlur(event, mod, "workshopID")} />
                )
            }
        },
        {
            key: "delete",
            name: "Delete",
            fieldName: "delete",
            minWidth: 80,
            onRender: (mod: Mod) => {
                return (
                    <DefaultButton
                        onClick={() => deleteServerMod(mod.id)}
                        className="Button"
                    >
                        <DeleteIcon />
                    </DefaultButton>
                )
            }
        }
    ]

    let texts: JSX.Element[] = new Array;
    if (managerConfig != null) {
        Object.entries(managerConfig).map(([key, value]) => (key != "serverMods" && key != "clientMods" && key != "customMessages") && texts.push(<TextField key={key} id={key} label={key} value={value} onChange={handleChange} />));
    }

    const contents = managerConfig === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <div>
            <div style={{ display: "flex", flexDirection: "column" }}>
                {
                    texts.map(x => { return x; })
                }
            </div>
            <h3>Client Mods</h3>
            <div>
                <DefaultButton onClick={() => { createClientMod() }} className="Button">
                    Add new Row
                </DefaultButton>
                <DetailsList
                    setKey="clientModsDetailsList"
                    items={managerConfig.clientMods}
                    columns={clientModsColumns}
                    selection={clientModsSelection}
                    checkboxVisibility={CheckboxVisibility.always}
                    dragDropEvents={dragDropEvents}
                    onRenderRow={(props?: IDetailsRowProps) => {
                        if (!props) return null;
                        return (
                            <div
                                draggable
                                onDragStart={() => handleClientModsDragStart(props.item, props.itemIndex)}
                                onDrop={() => handleClientModsDrop(props.item)}
                                onDragOver={(event) => event.preventDefault()}
                                onDragEnd={() => handleClientModsDragEnd()}
                                onDragEnter={() => handleDragEnter(props.item)}
                                onDragLeave={() => handleDragLeave()}
                                key={(props.item as Mod).id}
                            >
                                <DetailsRow {...props} />
                            </div>
                        );
                    }}
                />
        </div>
            <h3>Server Mods</h3>
            <div>
                <DefaultButton
                    onClick={() => { createServerMod() }}
                    className="Button"
                >
                    Add new Row
                </DefaultButton>
                <DetailsList
                    setKey="serverModsDetailsList"
                    items={managerConfig.serverMods}
                    columns={serverModsColumns}
                    selection={serverModsSelection}
                    checkboxVisibility={CheckboxVisibility.always}
                    dragDropEvents={dragDropEvents}
                    onRenderRow={(props?: IDetailsRowProps) => {
                        if (!props) return null;
                        return (
                            <div
                                draggable
                                onDragStart={() => handleServerModsDragStart(props.item, props.itemIndex)}
                                onDrop={() => handleServerModsDrop(props.item)}
                                onDragOver={(event) => event.preventDefault()}
                                onDragEnd={() => handleServerModsDragEnd()}
                                onDragEnter={() => handleDragEnter(props.item)}
                                onDragLeave={() => handleDragLeave()}
                            >
                                <DetailsRow {...props} />
                            </div>
                        );
                    }}
                />
            </div>
            <h3>Custom Messages</h3>
            <div>
                {
                    managerConfig.customMessages.map((message, index) => {
                        return (
                            <div className="messageContainer" key={message.id} id={"Message-" + index}>
                                <DefaultButton onClick={() => { moveMessageUp(index) }} className="Button"><KeyboardArrowUpIcon /></DefaultButton>
                                <DefaultButton onClick={() => { moveMessageDown(index) }} className="Button"><KeyboardArrowDownIcon /></DefaultButton>
                                <div className="subContainer">
                                    <h4>Wait Time</h4>
                                    <TextField id="waitTime-hours" label="Hours" defaultValue={message.waitTime["hours"]} onChange={(event, newValue) => handleWaitTimeChange(event, index, newValue)} />
                                    <TextField id="waitTime-minutes" label="Minutes" defaultValue={message.waitTime["minutes"]} onChange={(event, newValue) => handleWaitTimeChange(event, index, newValue)} />
                                    <TextField id="waitTime-seconds" label="Seconds" defaultValue={message.waitTime["seconds"]} onChange={(event, newValue) => handleWaitTimeChange(event, index, newValue)} />
                                </div>
                                <div className="subContainer">
                                    <h4>Interval</h4>
                                    <TextField id="interval-hours" label="Hours" defaultValue={message.interval["hours"]} onChange={(event, newValue) => handleIntervalChange(event, index, newValue)} />
                                    <TextField id="interval-minutes" label="Minutes" defaultValue={message.interval["minutes"]} onChange={(event, newValue) => handleIntervalChange(event, index, newValue)} />
                                    <TextField id="interval-seconds" label="Seconds" defaultValue={message.interval["seconds"]} onChange={(event, newValue) => handleIntervalChange(event, index, newValue)} />
                                </div>
                                <div className="subContainer">
                                    <TextField id="Title" label="Title" defaultValue={message.title} onChange={(event, newValue) => handleCustomMessagesChange(event, index, newValue)} />
                                    <TextField multiline={true} id="Message" label="Message" defaultValue={message.message} onChange={(event, newValue) => handleCustomMessagesChange(event, index, newValue)} />
                                    <TextField id="Icon" label="Icon" defaultValue={message.icon} onChange={(event, newValue) => handleCustomMessagesChange(event, index, newValue)} />
                                    <TextField id="Color" label="Color" defaultValue={message.color} onChange={(event, newValue) => handleCustomMessagesChange(event, index, newValue)} />
                                    <TextField id="IsTimeOfDay" label="Is Time Of Day" defaultValue={message.isTimeOfDay.toString()} onChange={(event, newValue) => handleCustomMessagesChange(event, index, newValue)} />
                                </div>
                                <DefaultButton id="deleteButton" onClick={() => deleteCustomMessage(message.id)} className="Button">
                                    <DeleteIcon/>
                                </DefaultButton>
                            </div>
                        )
                    })
                }
                <DefaultButton
                    onClick={() => { createCustomMessage() }}
                    className="Button"
                >
                    Add new Row
                </DefaultButton>
            </div>
        </div>

    return (
        <div style={{padding: "10px 10px 10px 10px"} }>
            <h1 id="tableLabel">Manager Configurations</h1>
            <div style={{ display: "flex", flexDirection: "row" }}>
                <SaveButton
                    handleSave={handleSave}
                />
                <ReloadButton
                    handleLoad={handleLoad}
                />
            </div>
            <div>
                {contents}
            </div>
        </div>
    );
}

async function PopulateManagerConfig(setManagerConfig: Function, endpoint: string) {
    const response = await fetch(endpoint);
    const result = (await response.json()) as ManagerConfig;
    if (result != null) {
        setManagerConfig(result);
    }
}

async function PostManagerConfig(endpoint: string, json: string) {
    const response = await fetch(endpoint, {
        method: "POST",
        mode: "cors",
        headers: { 'Content-Type': 'application/json' },
        body: json
    });
    const result = await response.text();
    if (result != null) {
        alert(result);
    }
}