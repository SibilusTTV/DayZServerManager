
import { CheckboxVisibility, DefaultButton, DetailsList, DetailsRow, getTheme, IColumn, IDetailsRowProps, IDragDropContext, IDragDropEvents, initializeIcons, IObjectWithKey, mergeStyles, Selection, SelectionMode, TextField } from '@fluentui/react';
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
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
    const [draggedCustomMessages, setDraggedCustomMessages] = useState<CustomMessage>();
    const [draggedClientModsIndex, setDraggedClientModsIndex] = useState<number>(-1);
    const [draggedServerModsIndex, setDraggedServerModsIndex] = useState<number>(-1);
    const [draggedCustomMessagesIndex, setDraggedCustomMessagesIndex] = useState<number>(-1);
    const [, setSelectedClientMods] = useState<IObjectWithKey[]>();
    const [, setSelectedServerMods] = useState<IObjectWithKey[]>();
    const [, setSelectedCustomMessages] = useState<IObjectWithKey[]>();

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

    const customMessageSelection: Selection = useMemo(() => new Selection(
        {
            onSelectionChanged: () => {
                setSelectedCustomMessages(customMessageSelection.getSelection());
            },
            selectionMode: SelectionMode.multiple,
            getKey: (item) => (item as CustomMessage).id
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

    const handleCustomMessagesChange = (customMessage: CustomMessage, newValue: string | undefined, key: string) => {
        if (managerConfig) {
            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: managerConfig.customMessages.map((message) => message.id === customMessage.id ? {
                        ...customMessage,
                        [key]: newValue
                    } :
                        message)
                }
            )
        }
    }

    const handleWaitTimeChange = (customMessage: CustomMessage, newValue: string | undefined, key: string) => {
        if (managerConfig && newValue) {
            let newNumber = parseInt(newValue);
            if (!Number.isNaN(newNumber)) {
                if (newNumber > 24) {
                    newNumber = 24;
                }
                else if (newNumber < 0) {
                    newNumber = 0;
                }
            }
            else {
                newNumber = 0;
            }

            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: managerConfig.customMessages.map((message) => message.id === customMessage.id ? {
                        ...message,
                        waitTime: {
                            ...message.waitTime,
                            [key]: newNumber
                        }
                    } :
                        message)
                }
            )
        }
    }

    const handleIntervalChange = (customMessage: CustomMessage, newValue: string | undefined, key: string) => {
        if (managerConfig && newValue) {
            let newNumber = 0;
            if (!Number.isNaN(newValue)) {
                newNumber = parseInt(newValue);
                if (newNumber > 24) {
                    newNumber = 24;
                }
                else if (newNumber < 0) {
                    newNumber = 0;
                }
            }

            setManagerConfig(
                {
                    ...managerConfig,
                    customMessages: managerConfig.customMessages.map((message) => message.id === customMessage.id ? {
                        ...message,
                        interval: {
                            ...message.interval,
                            [key]: newNumber
                        }
                    } :
                        message)
                }
            )
        }
    }

    const deleteCustomMessage = (id: number) => {
        if (managerConfig) {
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

    const insertBeforeCustomMessages = (item: CustomMessage): void => {
        if (managerConfig) {
            const draggedItems = customMessageSelection.isIndexSelected(draggedCustomMessagesIndex)
                ? (customMessageSelection.getSelection() as CustomMessage[])
                : [draggedCustomMessages!];

            const items = managerConfig.customMessages.filter(itm => draggedItems.indexOf(itm) === -1);

            const insertIndex = managerConfig.customMessages.indexOf(item as CustomMessage);
            items.splice(insertIndex, 0, ...draggedItems);

            setManagerConfig({
                ...managerConfig,
                customMessages: items
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

    const handleCustomMessagesDragStart = (item: any, itemIndex: number) => {
        setDraggedCustomMessages(item);
        setDraggedCustomMessagesIndex(itemIndex!);
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

    const handleCustomMessagesDrop = (item: any) => {
        if (draggedCustomMessages) {
            insertBeforeCustomMessages(item);
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

    const handleCustomMessagesDragEnd = () => {
        setDraggedCustomMessages(undefined);
        setDraggedCustomMessagesIndex(-1);
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

    const customMessagesColumns: IColumn[] = [
        {
            key: "waitTime",
            name: "Wait Time",
            fieldName: "waitTime",
            minWidth: 200,
            maxWidth: 200,
            onRender: (customMessage: CustomMessage) => {
                return (
                    <div style={{ display: "flex", flexDirection: "row" }}>
                        <TextField
                            id="waitTime-hours"
                            label="Hours"
                            value={customMessage.waitTime["hours"]}
                            onChange={(_, newValue) => handleWaitTimeChange(customMessage, newValue, "hours")}
                        />
                        <TextField
                            id="waitTime-minutes"
                            label="Minutes"
                            value={customMessage.waitTime["minutes"]}
                            onChange={(_, newValue) => handleWaitTimeChange(customMessage, newValue, "minutes")}
                        />
                        <TextField id="waitTime-seconds"
                            label="Seconds"
                            value={customMessage.waitTime["seconds"]}
                            onChange={(_, newValue) => handleWaitTimeChange(customMessage, newValue, "seconds")}
                        />
                    </div>
                )
            }
        },
        {
            key: "interval",
            name: "Interval",
            fieldName: "interval",
            minWidth: 200,
            maxWidth: 200,
            onRender: (customMessage: CustomMessage) => {
                return (
                    <div style={{ display: "flex", flexDirection: "row" }}>
                        <TextField
                            id="interval-hours"
                            label="Hours"
                            value={customMessage.interval["hours"]}
                            onChange={(_, newValue) => handleIntervalChange(customMessage, newValue, "hours")}
                        />
                        <TextField
                            id="interval-minutes"
                            label="Minutes"
                            value={customMessage.interval["minutes"]}
                            onChange={(_, newValue) => handleIntervalChange(customMessage, newValue, "minutes")}
                        />
                        <TextField id="interval-seconds"
                            label="Seconds"
                            value={customMessage.interval["seconds"]}
                            onChange={(_, newValue) => handleIntervalChange(customMessage, newValue, "seconds")}
                        />
                    </div>
                )
            }
        },
        {
            key: "title",
            name: "Title",
            fieldName: "title",
            minWidth: 200,
            maxWidth: 200,
            onRender: (customMessage: CustomMessage, _, column: IColumn | undefined) => {
                return (
                    < TextField
                        id="Title"
                        label="Title"
                        value={customMessage.title}
                        onChange={(_, newValue) => handleCustomMessagesChange(customMessage, newValue, column?.fieldName || "")}
                    />
                )
            }
        },
        {
            key: "message",
            name: "Message",
            fieldName: "message",
            minWidth: 160,
            maxWidth: 640,
            onRender: (customMessage: CustomMessage, _, column: IColumn | undefined) => {
                return (
                    <TextField
                        multiline={true}
                        id="Message"
                        label="Message"
                        value={customMessage.message}
                        onChange={(_, newValue) => handleCustomMessagesChange(customMessage, newValue, column?.fieldName || "")}
                    />
                )
            }
        },
        {
            key: "icon",
            name: "Icon",
            fieldName: "icon",
            minWidth: 200,
            maxWidth: 200,
            onRender: (customMessage: CustomMessage, _, column: IColumn | undefined) => {
                return (
                    <TextField
                        id="Icon"
                        label="Icon"
                        value={customMessage.icon}
                        onChange={(_, newValue) => handleCustomMessagesChange(customMessage, newValue, column?.fieldName || "")}
                    />
                )
            }
        },
        {
            key: "color",
            name: "Color",
            fieldName: "color",
            minWidth: 200,
            maxWidth: 200,
            onRender: (customMessage: CustomMessage, _, column: IColumn | undefined) => {
                return (
                    <TextField
                        id="Color"
                        label="Color"
                        value={customMessage.color}
                        onChange={(_, newValue) => handleCustomMessagesChange(customMessage, newValue, column?.fieldName || "")}
                    />
                )
            }
        },
        {
            key: "isTimeOfDay",
            name: "Is Time Of Day",
            fieldName: "isTimeOfDay",
            minWidth: 200,
            maxWidth: 200,
            onRender: (customMessage: CustomMessage, _, column: IColumn | undefined) => {
                return (
                    <TextField
                        id="IsTimeOfDay"
                        label="Is Time Of Day"
                        value={customMessage.isTimeOfDay.toString()}
                        onChange={(_, newValue) => handleCustomMessagesChange(customMessage, newValue, column?.fieldName || "")}
                    />
                )
            }
        },
        {
            key: "delete",
            name: "Delete",
            fieldName: "delete",
            minWidth: 100,
            maxWidth: 100,
            onRender: (customMessage: CustomMessage) => {
                return (
                    <DefaultButton
                        id="deleteButton"
                        onClick={() => deleteCustomMessage(customMessage.id)}
                        className="Button">
                        <DeleteIcon />
                    </DefaultButton>
                )
            }
        }
    ];

    let texts: JSX.Element[] = new Array;
    if (managerConfig != null) {
        Object.entries(managerConfig).map(([key, value]) => (key != "serverMods" && key != "clientMods" && key != "customMessages") && texts.push(<TextField key={key} id={key} label={key} value={value} onChange={handleChange} />));
    };

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
                <DefaultButton
                    onClick={() => { createClientMod() }}
                    className="Button"
                    style={{ margin: "0px 10px 0px 0px" }}
                >
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
                    style={{ margin: "0px 10px 0px 0px" }}
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
                                key={(props.item as Mod).id}
                            >
                                <DetailsRow {...props} />
                            </div>
                        );
                    }}
                />
            </div>
            <div>
                <h3>Custom Messages</h3>
                <div>
                    <DefaultButton
                        onClick={() => { createCustomMessage() }}
                        className="Button"
                        style={{ margin: "0px 10px 0px 0px" }}
                    >
                        Add new Row
                    </DefaultButton>
                    <DetailsList
                        setKey="customMessagesDetailsList"
                        items={managerConfig.customMessages}
                        columns={customMessagesColumns}
                        selection={customMessageSelection}
                        checkboxVisibility={CheckboxVisibility.always}
                        dragDropEvents={dragDropEvents}
                        onRenderRow={(props?: IDetailsRowProps) => {
                            if (!props) return null;
                            return (
                                <div
                                    draggable
                                    onDragStart={() => handleCustomMessagesDragStart(props.item, props.itemIndex)}
                                    onDrop={() => handleCustomMessagesDrop(props.item)}
                                    onDragOver={(event) => event.preventDefault()}
                                    onDragEnd={() => handleCustomMessagesDragEnd()}
                                    onDragEnter={() => handleDragEnter(props.item)}
                                    onDragLeave={() => handleDragLeave()}
                                    key={(props.item as CustomMessage).id}
                                >
                                    <DetailsRow {...props} />
                                </div>
                            );
                        }}
                    />
                </div>
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