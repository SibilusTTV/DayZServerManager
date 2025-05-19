
import React, { useEffect, useMemo, useState } from "react";
import SaveButton from "../../common/components/save-button/SaveButton";
import ReloadButton from "../../common/components/reload-button/ReloadButton";
import { CheckboxVisibility, DefaultButton, DetailsList, DetailsRow, Dropdown, getTheme, IColumn, IDetailsRowProps, IDragDropEvents, IDropdownOption, initializeIcons, IObjectWithKey, MarqueeSelection, mergeStyles, Selection, SelectionMode, TextField } from "@fluentui/react";
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/DeleteOutlined';

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
    const [listItems, setListItems] = useState<ListItem[]>();
    const [draggedItem, setDraggedItem] = useState<ListItem>();
    const [draggedIndex, setDraggedIndex] = useState<number>(-1);
    const [, setSelectedItems] = useState<IObjectWithKey[]>();

    const badNamesSelection: Selection = useMemo(() => new Selection(
        {
            onSelectionChanged: () => {
                const newSelection = badNamesSelection.getSelection();
                setSelectedItems(newSelection);
            },
            selectionMode: SelectionMode.multiple,
            getKey: (item) => (item as ListItem).listId
        }),
        []
    );

    useEffect(() => {
        handleLoad();
    }, []);

    const handleLoad = () => {
        PopulateSchedulerManagerConfig(setSchedulerConfig, 'SchedulerConfig/GetSchedulerConfig', setListItems);
    };

    const handleSave = () => {
        PostSchedulerConfig('SchedulerConfig/PostSchedulerConfig', JSON.stringify(schedulerConfig));
    };

    initializeIcons();

    const columns: IColumn[] = [
        {
            key: "badName",
            name: "Bad Name",
            fieldName: "badName",
            minWidth: 200,
            onRender: (badName: ListItem, index: number | undefined) => {
                return (
                    <TextField
                        defaultValue={badName.badName}
                        onBlur={(event) => {
                            if (schedulerConfig && listItems && index != null) {
                                const items = listItems.map(item => item.listId == badName.listId ? { ...item, badName: event.target.value } : item)
                                setListItems(
                                    items
                                );
                                setSchedulerConfig(
                                    {
                                        ...schedulerConfig,
                                        badNames: items.map(itm => itm.badName)
                                    }
                                );
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
                                () => {
                                    if (schedulerConfig && listItems) {
                                        const items = [...listItems, { listId: getNextListItemId(), badName: "" }];
                                        setListItems(
                                            items
                                        );
                                        setSchedulerConfig(
                                            { ...schedulerConfig, badNames: items.map(item => item.badName) }
                                        );
                                    }
                                }
                            }
                            className="Button"
                            style={{margin: "5px 10px 5px 10px"} }
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
                            () => {
                                if (schedulerConfig && listItems) {
                                    const items = listItems.filter((item) => item.listId != badName.listId);
                                    setListItems(
                                        items
                                    );
                                    setSchedulerConfig(
                                        { ...schedulerConfig, badNames: items.map(item => item.badName) }
                                    );
                                }
                            }
                        }
                        className="Button"
                    >
                        <DeleteIcon />
                    </DefaultButton>
                )
            }
        }
    ];

    const getNextListItemId = () => {
        let index = 0
        if (listItems) {
            for (index; index < listItems.length; index++) {
                if (listItems.find(item => item.listId === index) == null) {
                    return index;
                }
            }
        }
        return index;
    }

    const handleFieldBlur = (event: React.FocusEvent<HTMLInputElement>) => {
        if (schedulerConfig) {
            setSchedulerConfig(
                {
                    ...schedulerConfig,
                    [event.target.id]: (event.target.value.toLowerCase() == "true") ? true : (event.target.value.toLowerCase() == "false" ? false : event.target.value)
                }
            );
        }
    }

    const handleDropDownChange = (key: string, option: IDropdownOption | undefined) => {
        if (option && schedulerConfig) {
            setSchedulerConfig(
                {
                    ...schedulerConfig,
                    [key]: option.text === "true"
                }
            )
        }
    }

    const insertBeforeName = (item: ListItem): void => {
        if (schedulerConfig && listItems) {
            const draggedItems = badNamesSelection.isIndexSelected(draggedIndex)
                ? (badNamesSelection.getSelection() as ListItem[])
                : [draggedItem!];

            const items = listItems.filter(itm => draggedItems.indexOf(itm) === -1);

            const insertIndex = listItems.indexOf(item as ListItem);
            items.splice(insertIndex, 0, ...draggedItems);

            setListItems(items);

            setSchedulerConfig({
                ...schedulerConfig,
                badNames: items.map(itm => itm.badName)
            });
        }
    };

    const handleDragStart = (item: any, itemIndex: number) => {
        setDraggedItem(item);
        setDraggedIndex(itemIndex!);
    };

    const handleDrop = (item: any) => {
        if (draggedItem) {
            insertBeforeName(item);
        }
    };

    const handleDragEnd = () => {
        setDraggedItem(undefined);
        setDraggedIndex(-1);
    };

    const handleDragEnter = () => {
        // return string is the css classes that will be added to the entering element.
        return mergeStyles({
            backgroundColor: getTheme().palette.neutralLight,
        });
    };

    const handleDragLeave = () => {
        return;
    };

    const badNamesDragDropEvents: IDragDropEvents = {
        canDrop: () => {
            return true;
        },
        canDrag: (_item?: any) => {
            return true;
        }
    };

    const options: IDropdownOption[] = [
        {
            key: "true",
            text: "true"
        },
        {
            key: "false",
            text: "false"
        }
    ];

    return (
        <div style={{padding: "10px 10px 10px 10px"} }>
            <h1 id="tableLabel">Scheduler Configurations</h1>
            <div key="buttons" style={{ display: "flex", flexDirection: "row", gap: "10px" }}>
                <SaveButton
                    handleSave={handleSave}
                />
                <ReloadButton
                    handleLoad={handleLoad}
                />
            </div>
            <div key="configFields" style={{ padding: "10px 0px 0px 0px", flexDirection: "row", display: "flex", gap: "10px" }}>
                {schedulerConfig && Object.entries(schedulerConfig).map(([key, value]) => {
                    if (key === "useWhiteList" || key === "useNickFilter") {
                        return (
                            <Dropdown
                                key={key}
                                selectedKey={value.toString()}
                                onChange={(_, option) => handleDropDownChange(key, option)}
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
                                onBlur={handleFieldBlur}
                                defaultValue={value}
                                styles={{ root: { width: 400, padding: "0px", margin: "0px" } }}
                            />
                        )
                    }
                })}
            </div>
            <div key="badNamesList" style={{ padding: "10px 0px 0px 0px", flexDirection: "column", display: "flex" }}>
                <MarqueeSelection selection={badNamesSelection}>
                    <DetailsList
                        setKey="badNamesDetailsList"
                        styles={{ root: { padding: "10px 0px 0px 0px" } }}
                        items={listItems || []}
                        columns={columns}
                        selection={badNamesSelection}
                        checkboxVisibility={CheckboxVisibility.always}
                        dragDropEvents={badNamesDragDropEvents}
                        ariaLabelForSelectionColumn="Toggle selection"
                        ariaLabelForSelectAllCheckbox="Toggle selection for all items"
                        checkButtonAriaLabel="select row"
                        onRenderRow={(props?: IDetailsRowProps) => {
                            if (!props) return null;
                            return (
                                <div
                                    draggable
                                    onDragStart={() => handleDragStart(props.item, props.itemIndex)}
                                    onDrop={() => handleDrop(props.item)}
                                    onDragOver={(event) => event.preventDefault()}
                                    onDragEnd={() => handleDragEnd()}
                                    onDragEnter={() => handleDragEnter()}
                                    onDragLeave={() => handleDragLeave()}
                                    key={(props.item as ListItem).listId}
                                >
                                    <DetailsRow {...props} />
                                </div>
                            );
                        }}
                    />
                </MarqueeSelection>
            </div>
        </div>
    )
}

async function PopulateSchedulerManagerConfig(setSchedulerConfig: Function, endpoint: string, setItems: Function) {
    const response = await fetch(endpoint);
    const result = (await response.json()) as SchedulerConfig;
    if (result != null) {
        setSchedulerConfig(result);
        setItems(result.badNames.map((badName, index) => { return { listId: index, badName: badName } }));
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
