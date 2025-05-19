
import { useEffect, useMemo, useState } from "react";
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import AddIcon from '@mui/icons-material/Add';
import SaveButton from "../../common/components/save-button/SaveButton";
import ReloadButton from "../../common/components/reload-button/ReloadButton";
import { CheckboxVisibility, ColumnActionsMode, ContextualMenu, DefaultButton, DetailsList, DetailsRow, DirectionalHint, Dropdown, getTheme, IColumn, IContextualMenuItem, IContextualMenuProps, IDetailsRowProps, IDragDropContext, IDragDropEvents, IDropdownOption, initializeIcons, IObjectWithKey, mergeStyles, Selection, SelectionMode, TextField } from "@fluentui/react";

interface ServerConfig {
    properties: PropertyValue[]
}

interface PropertyValue {
    id: number,
    propertyName: string,
    dataType: DataType,
    value: Object | string | Number | Boolean | undefined,
    comment: string
}

enum DataType {
    Number = 0,
    Text = 1,
    TextLong = 2,
    TextShort = 3,
    Decimal = 4,
    Boolean = 5,
    Array = 6
}

interface StringRow {
    id: number,
    lineNumber: string
    lineString: string
}

interface Dictionary<T> {
    [key: string]: T;
}

const dataTypeDropdownOptions: IDropdownOption[] = [
    {
        key: 0,
        text: "Number"
    },
    {
        key: 1,
        text: "Text (128)"
    },
    {
        key: 2,
        text: "TextLong (256)"
    },
    {
        key: 3,
        text: "TextShort (64)"
    },
    {
        key: 4,
        text: "Decimal"
    },
    {
        key: 5,
        text: "Boolean"
    }
    //{
    //    key: 6,
    //    text: "Array"
    //}
]

const booleanDropdown: IDropdownOption[] = [
    {
        key: "false",
        text: "False"
    },
    {
        key: "true",
        text: "True"
    }
]

export default function ServerConfigEditor() {
    const [serverConfig, setServerConfig] = useState<ServerConfig>();
    const [sortedPropertyValues, setSortedPropertyValues] = useState<PropertyValue[]>();
    const [motdArray, setMotdArray] = useState<StringRow[]>();
    const [, setSelectedArrayRow] = useState<IObjectWithKey[]>();
    const [contextualMenuProps, setContextualMenuProps] = useState<IContextualMenuProps>();
    const [sortKey, setSortKey] = useState("");
    const [isSortedDescending, setIsSortedDescending] = useState(false);
    const [fieldFilters, setFieldFilters] = useState<Dictionary<string>>({ "id": "", "propertyName": "", "dataType": "", "value": "", "comment": "" });

    const [motdPropertyId, setMotdPropertyId] = useState<number>();
    const [draggedMotdLines, setDraggedMotdLines] = useState<StringRow>();
    const [draggedMotdLinesIndex, setDraggedMotdLinesIndex] = useState<number>(-1);

    const arraySelection: Selection = useMemo(() => new Selection(
        {
            onSelectionChanged: () => {
                setSelectedArrayRow(arraySelection.getSelection());
            },
            selectionMode: SelectionMode.multiple,
            getKey: (item) => (item as StringRow).id
        }),
        []
    );

    initializeIcons();

    useEffect(() => {
        handleLoad();
    }, []);

    const handleLoad = () => {
        populateServerConfig(setServerConfig, 'ServerConfig/GetServerConfig', setSortedPropertyValues, setMotdPropertyId, setMotdArray);
    };

    const handleSave = () => {
        postServerConfig('ServerConfig/PostServerConfig', JSON.stringify(serverConfig))
    };

    const onColumnContextMenu = (column: IColumn | undefined, ev: React.MouseEvent<HTMLElement> | undefined): void => {
        if (column && ev && column.columnActionsMode !== ColumnActionsMode.disabled) {
            setContextualMenuProps(getContextualMenuProps(column, ev));
        }
    }

    const onColumnClick = (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
        if (column.columnActionsMode !== ColumnActionsMode.disabled) {
            setContextualMenuProps(getContextualMenuProps(column, ev));
        }
    }

    const arrayColumns: IColumn[] = [
        {
            key: 'lineNumber',
            fieldName: 'lineNumber',
            name: 'Line Number',
            minWidth: 160,
            maxWidth: 320,
            onRender: (item: StringRow) => {
                return (
                    <div>
                        {item.lineNumber}
                    </div>
                );
            }
        },
        {
            key: 'lineString',
            fieldName: 'lineString',
            name: 'Content',
            minWidth: 360,
            onRender: (item: StringRow, _, column: IColumn | undefined) => {
                return (
                    <TextField
                        key={"TextField-" + item.id + "-" + (column?.key || "")}
                        defaultValue={item.lineString}
                        onBlur={(event) => handleArrayFieldBlur(event, item)}
                    />
                );
            }
        },
        {
            key: 'delete',
            fieldName: 'delete',
            name: 'Delete',
            minWidth: 80,
            onRender: (item: StringRow) => {
                return (
                    <DefaultButton
                        label="Delete"
                        onClick={() => deleteArrayItem(item)}
                        className="Button"
                    >
                        <DeleteIcon />
                    </DefaultButton>
                )
            }
        }
    ]

    const columns: IColumn[] = [
        {
            key: 'propertyName',
            fieldName: 'propertyName',
            name: 'Property Name',
            minWidth: 160,
            maxWidth: 320,
            isResizable: true,
            isSorted: sortKey === "propertyName",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["propertyName"] != "",
            onRender: (item: PropertyValue, _, column: IColumn | undefined) => {
                return (
                    <TextField
                        key={"TextField-" + item.id + "-" + (column?.key || "")}
                        defaultValue={item.propertyName}
                        onBlur={(event) => handleFieldBlur(event, item, column?.key || "")}
                    />
                )
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown
        },
        {
            key: 'dataType',
            fieldName: 'dataType',
            name: 'Data Type',
            minWidth: 160,
            maxWidth: 160,
            isSorted: sortKey === "dataType",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["dataType"] != "",
            onRender: (item: PropertyValue, _, column: IColumn | undefined) => {
                return (
                    <Dropdown
                        key={"DropDown-" + item.id + "-" + (column?.key || "")}
                        selectedKey={item.dataType}
                        options={dataTypeDropdownOptions}
                        onChange={(_, option) => handleDataTypeSelect(option, item)}
                    />
                )
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown
        },
        {
            key: 'value',
            fieldName: 'value',
            name: 'Value',
            minWidth: 160,
            maxWidth: 320,
            isSorted: sortKey === "value",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["value"] != "",
            onRender: (item: PropertyValue, _, column: IColumn | undefined) => {
                if (item.dataType != DataType.Boolean) {
                    return (
                        <TextField
                            key={"TextField-" + item.id + "-" + (column?.key || "")}
                            defaultValue={item.value?.toString()}
                            onBlur={(event) => handleValueFieldBlur(event, item)}
                        />
                    )
                }
                else {
                    return(
                        <Dropdown
                            key={"DropDown-" + item.id + "-" + (column?.key || "")}
                            selectedKey={item.value?.toString()}
                            options={booleanDropdown}
                            onChange={(_, option) => option && handleBooleanSelect(option, item)}
                        />
                    )
                }
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown
        },
        {
            key: 'comment',
            fieldName: 'comment',
            name: 'Comment',
            minWidth: 320,
            isSorted: sortKey === "comment",
            isSortedDescending: isSortedDescending,
            isFiltered: fieldFilters["comment"] != "",
            onRender: (item: PropertyValue, _, column: IColumn | undefined) => {
                return (
                    <TextField
                        key={"TextField-" + item.id + "-" + (column?.key || "")}
                        defaultValue={item.comment}
                        onBlur={(event) => handleFieldBlur(event, item, column?.key || "") }
                    />
                )
            },
            onColumnContextMenu: onColumnContextMenu,
            onColumnClick: onColumnClick,
            columnActionsMode: ColumnActionsMode.hasDropdown 
        },
        {
            key: 'delete',
            fieldName: 'delete',
            name: 'Delete',
            minWidth: 120,
            maxWidth: 120,
            onRender: (item: PropertyValue) => {
                return (
                    <DefaultButton
                        label="Delete"
                        onClick={() => deleteProperty(item.id)}
                        className="Button"
                    >
                        <DeleteIcon />
                    </DefaultButton>
                )
            }
        }
    ];

    const booleanify = (value: string): boolean => {
        const truthy: string[] = [
            'true',
            'True',
            '1'
        ]

        return truthy.includes(value)
    }

    const deleteProperty = (id: number) => {
        if (serverConfig != null) {
            setServerConfig(
                {
                    ...serverConfig,
                    properties: serverConfig.properties.filter(x => x.id != id)
                }
            );
        }
    }

    const handleDataTypeSelect = (newValue: IDropdownOption<any> | undefined, row: PropertyValue) => {
        if (serverConfig && newValue) {
            setServerConfig(
                {
                    properties: serverConfig.properties.map((property) => {
                        if (property.id == row.id) {
                            let dataType = newValue.key as DataType;
                            let newFormattedValue;
                            switch (dataType) {
                                case DataType.Number:
                                    newFormattedValue = property.value && (!Number.isNaN(property.value) ? property.value : parseInt(property.value.toString()));
                                    break;
                                case DataType.Text:
                                    newFormattedValue = property.value && (typeof property.value === "string" ? property.value.substring(0, 128) : property.value.toString().substring(0, 128));
                                    break;
                                case DataType.TextLong:
                                    newFormattedValue = property.value && (typeof property.value === "string" ? property.value.substring(0, 256) : property.value.toString().substring(0, 256));
                                    break;
                                case DataType.TextShort:
                                    newFormattedValue = property.value && (typeof property.value === "string" ? property.value.substring(0, 64) : property.value.toString().substring(0, 64));
                                    break;
                                case DataType.Decimal:
                                    newFormattedValue = property.value && (!Number.isNaN(property.value) ? property.value : parseFloat(property.value.toString()));
                                    break;
                                case DataType.Boolean:
                                    newFormattedValue = property.value && booleanify(property.value.toString());
                                    break;
                                case DataType.Array:
                                    newFormattedValue = property.value;
                                    if (!Array.isArray(newFormattedValue)) {
                                        newFormattedValue = [""];
                                    }
                                    break;
                            }
                            return {
                                ...property,
                                dataType: dataType,
                                value: newFormattedValue
                            }
                        }
                        else {
                            return property;
                        }
                    })
                }
            )
        }
    }

    const handleBooleanSelect = (option: IDropdownOption<any>, item: PropertyValue) => {
        if (serverConfig) {
            const newItems = serverConfig.properties.map(oldItem => oldItem.id === item.id ? { ...item, value: option.key } : oldItem);
            setServerConfig({
                ...serverConfig,
                properties: newItems
            })
        }
    }

    const handleFieldBlur = (event: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement, Element>, item: PropertyValue, key: string) => {
        if (serverConfig) {
            setServerConfig({
                ...serverConfig,
                properties: serverConfig.properties.map(property => property.id === item.id ? { ...item, [key]: event.target.value } : property)
            });
        }
    };

    const handleValueFieldBlur = (event: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement, Element>, item: PropertyValue) => {
        if (serverConfig && event.target.value != item.value) {
            const newFormattedValue = formatValue(event.target.value, item.dataType);

            setServerConfig((prevConfig) => (prevConfig && {
                ...prevConfig,
                properties: prevConfig.properties.map((row) =>
                    row.propertyName === item.propertyName ? { ...item, value: newFormattedValue } : row
                ),
            }));

            return newFormattedValue;
        }
    };

    const formatValue = (value: string, dataType: DataType): any => {
        switch (dataType) {
            case DataType.Number:
                return isNaN(parseInt(value)) ? 0 : parseInt(value);
            case DataType.Decimal:
                return isNaN(parseFloat(value)) ? 0.0 : parseFloat(value);
            case DataType.Boolean:
                return booleanify(value);
            case DataType.Array:
                return Array.isArray(value) ? value : [""];
            case DataType.TextShort:
                return value.substring(0, 64);
            case DataType.Text:
                return value.substring(0, 128);
            case DataType.TextLong:
                return value.substring(0, 256);
            default:
                return value;
        }
    };

    const deleteArrayItem = (deletedRow: StringRow) => {
        if (serverConfig && motdArray) {
            const newMotdArray = motdArray.filter(line => line.id != deletedRow.id);

            setMotdArray(newMotdArray);

            setServerConfig(
                {
                    ...serverConfig,
                    properties: serverConfig.properties.map((property) => 
                        property.id == motdPropertyId ?
                            { ...property, value: newMotdArray.map(line => line.lineString) }
                            : property
                    )
                }
            );
        }
    };

    const handleArrayFieldBlur = (event: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement, Element>, item: StringRow) => {
        if (serverConfig && motdArray) {
            const newMotdArray = motdArray.map(line => line.id === item.id ? { ...line, lineString: event.target.value } : line);

            setMotdArray(newMotdArray);

            setServerConfig({
                ...serverConfig,
                properties: serverConfig.properties.map(property => property.id === motdPropertyId ?
                    { ...property, value: newMotdArray.map(line => line.lineString) }
                    : property
                )
            });
        }
    }

    const GetNextPropertyId = () => {
        if (serverConfig != null) {
            let index: number;
            for (index = 0; index < serverConfig.properties.length; index++) {
                if (serverConfig.properties.find(x => x.id == index) == null) {
                    return index;
                }
            }
            return index++;
        }
        return 0;
    }

    const handleAddProperty = () => {
        if (serverConfig != null) {
            setServerConfig(
                {
                    ...serverConfig,
                    properties: [
                        ...serverConfig.properties,
                        { id: GetNextPropertyId(), propertyName: "", dataType: DataType.Text, value: "", comment: "" }
                    ]
                }
            )
        }
    }

    const GetNextArrayId = () => {
        if (motdArray != null) {
            let index: number;
            for (index = 0; index < motdArray.length; index++) {
                if (motdArray.find(x => x.id == index) == null) {
                    return index;
                }
            }
            return index++;
        }
        return 0;
    }

    const handleAddArrayItem = (propertyId: number) => {
        if (serverConfig && motdArray) {
            const id = GetNextArrayId();
            const newMotdArray = [...motdArray, { id: id, lineNumber: "Line " + (id + 1), lineString: "" }]

            setMotdArray(newMotdArray)

            setServerConfig(
                {
                    ...serverConfig,
                    properties: serverConfig.properties.map(property =>
                        property.id == propertyId ?
                            { ...property, value: newMotdArray.map(line => line.lineString) }
                            : property
                    )
                }
            )
        }
    }

    const onContextualMenuDismissed = (): void => {
        setContextualMenuProps(undefined);
    }

    const getContextualMenuProps = (column: IColumn, ev: React.MouseEvent<HTMLElement>): IContextualMenuProps => {
        const items: IContextualMenuItem[] = [
            {
                key: 'aToZ',
                name: 'A to Z',
                iconProps: { iconName: 'SortUp' },
                canCheck: true,
                checked: column.isSorted && !column.isSortedDescending,
                onClick: () => {
                    column.isSorted && !column.isSortedDescending ? _onDisableSorting() : _onSortColumn(column, false);
                }
            },
            {
                key: 'zToA',
                name: 'Z to A',
                iconProps: { iconName: 'SortDown' },
                canCheck: true,
                checked: column.isSorted && column.isSortedDescending,
                onClick: () => {
                    column.isSorted && column.isSortedDescending ? _onDisableSorting() : _onSortColumn(column, true);
                }
            }
        ];

        const textfieldFilters: IContextualMenuItem[] = [
            {
                key: "clearFilter",
                name: "ClearFilter",
                iconProps: { iconName: 'Filter' },
                canCheck: true,
                checked: column.isFiltered,
                onClick: () => {
                    onFilterChange("", column);
                }
            },
            {
                key: 'filter',
                name: 'Filter',
                iconProps: { iconName: 'Filter' },
                canCheck: false,
                onRender: () => {
                    return <TextField label="Filter" defaultValue={fieldFilters[column.key]} onChange={(event) => onFilterChange(event.currentTarget.value, column)} />
                }
            }
        ];

        const dropdownFilters: IContextualMenuItem[] = [
            {
                key: "clearFilter",
                name: "ClearFilter",
                iconProps: { iconName: 'Filter' },
                canCheck: true,
                checked: column.isFiltered,
                onClick: () => {
                    onFilterChange("", column);
                }
            },
            {
                key: 'filter',
                name: 'Filter',
                iconProps: { iconName: 'Filter' },
                canCheck: false,
                onRender: () => {
                    return <Dropdown options={dataTypeDropdownOptions} label="Filter" defaultSelectedKey={(fieldFilters["dataType"] && fieldFilters["dataType"] != null) ? Number(fieldFilters["dataType"]) : undefined} onChange={(_, option) => option && onFilterChange(option.key.toString(), column)} />
                }
            }

        ];

        if (column.key != "dataType") {
            textfieldFilters.forEach(filter => items.push(filter));
        }
        else {
            dropdownFilters.forEach(filter => items.push(filter));
        }

        return {
            items: items,
            target: ev.currentTarget as HTMLElement,
            directionalHint: DirectionalHint.bottomLeftEdge,
            gapSpace: 0,
            isBeakVisible: true,
            onDismiss: onContextualMenuDismissed,
        }
    }

    const _onSortColumn = (column: IColumn, isSortedDescending: boolean) => {
        if (sortedPropertyValues && serverConfig) {
            const filteredAndSortedPropertyValues = _copyAndSort<PropertyValue>(sortedPropertyValues, column.key, isSortedDescending);
            setSortedPropertyValues(filteredAndSortedPropertyValues);

            setSortKey(column.key);
            setIsSortedDescending(isSortedDescending);
        }
    };

    const _onDisableSorting = () => {
        if (serverConfig) {
            setSortedPropertyValues(serverConfig.properties.filter(item => item.dataType != DataType.Array));

            setSortKey("");
            setIsSortedDescending(false);
        }
    }

    const onFilterChange = (newFilter: string, column: IColumn) => {
        if (serverConfig) {
            const newFieldFilters = {
                ...fieldFilters,
                [column.key]: newFilter
            };

            setFieldFilters(newFieldFilters);

            const filteredItemRarities = serverConfig.properties.filter(property => property.dataType != DataType.Array).filter(
                property => {
                    return getValidForFilters(property, newFieldFilters);
                }
            );

            const filteredAndSortedPropertyValues = _copyAndSort<PropertyValue>(filteredItemRarities, sortKey, isSortedDescending);

            setSortedPropertyValues(filteredAndSortedPropertyValues);
        }
    }

    const getValidForFilters = (property: PropertyValue, newFieldFilters: Dictionary<string>) => {
        for (let key in property) {
            const propertyKey = key as keyof typeof property;
            if (!key || (newFieldFilters[key] && newFieldFilters[key] != "") && !property[propertyKey]?.toString().toLowerCase().includes(newFieldFilters[key].toLowerCase())) {
                return false;
            }
        }
        return true;
    };

    const insertBeforeMotdLine = (item: StringRow): void => {
        if (motdArray && serverConfig && motdPropertyId) {
            const isIndexSelected = arraySelection.isIndexSelected(draggedMotdLinesIndex);
            const draggedItems = isIndexSelected
                ? (arraySelection.getSelection() as StringRow[])
                : [draggedMotdLines!];

            const items = motdArray.filter(itm => draggedItems.indexOf(itm) === -1);

            const insertIndex = motdArray.indexOf(item as StringRow);
            items.splice(insertIndex, 0, ...draggedItems);

            setMotdArray(items);

            setServerConfig({
                ...serverConfig,
                properties: serverConfig.properties.map(property => property.id === motdPropertyId ?
                    { ...property, value: items.map(item => item.lineString) }
                    : property
                )
            })
        }
    };

    const handleMotdLinesDragStart = (item: any, itemIndex: number) => {
        setDraggedMotdLines(item);
        setDraggedMotdLinesIndex(itemIndex!);
    };

    const handleMotdLinesDrop = (item: any) => {
        if (draggedMotdLines) {
            insertBeforeMotdLine(item);
        }
    };

    const handleMotdLinesDragEnd = () => {
        setDraggedMotdLines(undefined);
        setDraggedMotdLinesIndex(-1);
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

    const contents = serverConfig === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        :
        <div style={{ display: "flex", flexDirection: "column", gap: "10px" }}>
            {contextualMenuProps && <ContextualMenu {...contextualMenuProps} />}
            <DefaultButton
                onClick={() => handleAddProperty()}
                className="Button"
            >
                <AddIcon/>
            </DefaultButton>
            <DetailsList
                setKey="propertyValuesListBox"
                styles={{ root: { display: "flex" } }}
                items={sortedPropertyValues || []}
                columns={columns}
                selectionMode={SelectionMode.none}
            />
            {motdPropertyId &&
                <div key="motd" style={{display: "flex", gap: "10px"} }>
                    <h3>Motto of the day</h3>
                    <DefaultButton
                        onClick={() => handleAddArrayItem(motdPropertyId)}
                        className="Button"
                    >
                        <AddIcon/>
                    </DefaultButton>
                    <DetailsList
                        items={motdArray || []}
                        columns={arrayColumns}
                        selection={arraySelection}
                        checkboxVisibility={CheckboxVisibility.always}
                        dragDropEvents={dragDropEvents}
                        onRenderRow={(props?: IDetailsRowProps) => {
                            if (!props) return null;
                            return (
                                <div
                                    draggable
                                    onDragStart={() => handleMotdLinesDragStart(props.item, props.itemIndex)}
                                    onDrop={() => handleMotdLinesDrop(props.item)}
                                    onDragOver={(event) => event.preventDefault()}
                                    onDragEnd={() => handleMotdLinesDragEnd()}
                                    onDragEnter={() => handleDragEnter(props.item)}
                                    onDragLeave={() => handleDragLeave()}
                                    key={(props.item as StringRow).id}
                                >
                                    <DetailsRow {...props} />
                                </div>
                            );
                        }}
                    />
                </div>
            }
        </div>

    return (
        <div style={{ padding: "10px 10px 10px 10px", display: "flex", flexDirection: "column", gap: "10px" } }>
            <h1 id="tableLabel">Server Configurations</h1>
            <div style={{ display: "flex", flexDirection: "row", gap: "10px" }}>
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
    )
}

async function populateServerConfig(setServerConfig: Function, endpoint: string, setSortedPropertyValues: Function, setMotdProperty: Function, setMotdArray: Function) {
    const response = await fetch(endpoint);
    const result = (await response.json()) as ServerConfig;
    if (result != null) {
        setServerConfig(result);
        setSortedPropertyValues(result.properties.filter(item => item.dataType != DataType.Array));
        const motdProperty = result.properties.find(property => property.dataType === DataType.Array && property.propertyName === "motd[]");
        if (motdProperty) {
            setMotdProperty(motdProperty.id);
            setMotdArray((motdProperty.value as string[]).map((line, index) => ({ id: index, lineNumber: "Line " + (index + 1), lineString: line })));
        }
    }
}

async function postServerConfig(endpoint: string, data: string) {
    const response = await fetch(endpoint, {
        method: "POST",
        mode: "cors",
        headers: { 'Content-Type': 'application/json' },
        body: data
    });
    const result = await response.text()
    if (result != null) {
        alert(result);
    }
}

function _copyAndSort<T>(items: T[], columnKey: string, isSortedDescending?: boolean): T[] {
    const key = columnKey as keyof T;
    return items.slice(0).sort((a: T, b: T) => ((isSortedDescending ? a[key] < b[key] : a[key] > b[key]) ? 1 : -1));
}