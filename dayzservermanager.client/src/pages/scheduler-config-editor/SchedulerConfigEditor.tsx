
import React, { useEffect, useState } from "react";
import SaveButton from "../../common/components/save-button/SaveButton";
import ReloadButton from "../../common/components/reload-button/ReloadButton";
import { DefaultButton, DetailsList, Dropdown, IColumn, IDropdownOption, initializeIcons, SelectionMode, TextField } from "@fluentui/react";
import AddIcon from '@mui/icons-material/Add';
import CloseIcon from '@mui/icons-material/Close';

interface SchedulerConfig{
    useNickFilter: boolean;
    filteredNickMsg: string;
    badNames: string[];
    timeout: number;
}

interface ListItem {
    listId: number;
    badName: string;
}

export default function SchedulerConfigEditor() {
    const [schedulerConfig, setSchedulerConfig] = useState<SchedulerConfig>();

    useEffect(() => {
        PopulateSchedulerManagerConfig(setSchedulerConfig, 'SchedulerConfig/GetSchedulerConfig');
    }, []);

    initializeIcons();

    const columns: IColumn[] = [
        {
            key: "badName",
            name: "Bad Name",
            fieldName: "badName",
            minWidth: 200,
            onRender: (badName: ListItem) => {
                return (
                    <TextField
                        value={badName.badName}
                        onChange={(event) => {
                            if (schedulerConfig) {
                                setSchedulerConfig(
                                    {
                                        ...schedulerConfig,
                                        badNames: [
                                            ...schedulerConfig.badNames.slice(0, badName.listId),
                                            event.currentTarget.value.toString(),
                                            ...schedulerConfig.badNames.slice(badName.listId + 1)
                                        ]
                                    }
                                )
                            }
                        }}
                    />
                )
            },
            onRenderHeader: () => {
                return (
                    <div style={{display: "flex", flexDirection: "row"}}>
                        <span>Bad Name</span>
                        <DefaultButton
                            onClick={
                                () => { schedulerConfig && setSchedulerConfig({ ...schedulerConfig, badNames: [...schedulerConfig.badNames, ""] }) }
                            }
                            styles={{ root: { margin: "0px 10px 0px 10px" } }}
                        >
                            <AddIcon/>
                        </DefaultButton>
                    </div>
                )
            }
        },
        {
            key: "remove",
            name: "Remove",
            fieldName: "remove",
            minWidth: 80,
            onRender: (badName: ListItem) => {
                return (
                    <DefaultButton
                        onClick={
                            () => { schedulerConfig && setSchedulerConfig({ ...schedulerConfig, badNames: schedulerConfig.badNames.filter((_, index) => index != badName.listId) }) }
                        }
                    >
                        <CloseIcon/>
                    </DefaultButton>
                )
            }
        }
    ];

    const handleBlur = (event: React.FocusEvent<HTMLInputElement>) => {
        if (schedulerConfig) {
            setSchedulerConfig(
                {
                    ...schedulerConfig,
                    [event.target.id]: (event.target.value.toLowerCase() == "true") ? true : (event.target.value.toLowerCase() == "false" ? false : event.target.value)
                }
            );
        }
    }

    const handleChange = (key: string, option: IDropdownOption | undefined) => {
        if (option && schedulerConfig) {
            setSchedulerConfig(
                {
                    ...schedulerConfig,
                    [key]: option.text === "true"
                }
            )
        }
    }

    const options: IDropdownOption[] = [
        {
            key: "true",
            text: "true"
        },
        {
            key: "false",
            text: "false"
        }
    ] 

    return (
        <div style={{padding: "10px 10px 10px 10px"} }>
            <h1 id="tableLabel">Scheduler Configurations</h1>
            <div key="buttons">
                <SaveButton
                    postFunction={PostSchedulerConfig}
                    data={JSON.stringify(schedulerConfig)}
                    endpoint='SchedulerConfig/PostSchedulerConfig'
                />
                <ReloadButton
                    populateFunction={PopulateSchedulerManagerConfig}
                    setFunction={setSchedulerConfig}
                    endpoint='SchedulerConfig/GetSchedulerConfig'
                />
            </div>
            <div key="configFields" style={{ padding: "10px 0px 0px 0px", flexDirection: "row", display: "flex" }}>
                {schedulerConfig && Object.entries(schedulerConfig).map(([key, value]) => {
                    if (key === "useWhiteList" || key === "useNickFilter") {
                        return (
                            <Dropdown
                                selectedKey={value.toString()}
                                onChange={(_, option) => handleChange(key, option)}
                                label={key}
                                options={options}
                                styles={{ root: { width: 100, padding: "0px", margin: "0px" }}}
                            />
                        )
                    }
                    else if (key !== "badNames") {
                        return (
                            <TextField
                                key={key}
                                id={key}
                                label={key}
                                onBlur={handleBlur}
                                defaultValue={value}
                                styles={{ root: { width: 400, padding: "0px", margin: "0px" } }}
                            />
                        )
                    }
                })}
            </div>
            <div key="badNamesList" style={{ padding: "10px 0px 0px 0px", flexDirection: "column", display: "flex" }}>
                <DetailsList
                    styles={{ root: { padding: "10px 0px 0px 0px" } }}
                    items={schedulerConfig?.badNames.map((badName, index) => { return { listId: index, badName: badName } }) || []}
                    columns={columns}
                    selectionMode={SelectionMode.none}
                />
            </div>
        </div>
    )
}

async function PopulateSchedulerManagerConfig(setSchedulerConfig: Function, endpoint: string) {
    const response = await fetch(endpoint);
    const result = (await response.json()) as SchedulerConfig;
    if (result != null) {
        setSchedulerConfig(result);
    }
}

async function PostSchedulerConfig(endpoint: string, json: string) {
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
