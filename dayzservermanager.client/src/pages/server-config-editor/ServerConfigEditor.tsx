
import { useEffect, useMemo, useState } from "react";
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import AddIcon from '@mui/icons-material/Add';
import SaveButton from "../../common/components/save-button/SaveButton";
import ReloadButton from "../../common/components/reload-button/ReloadButton";
import { DefaultButton, DetailsList, Dropdown, IColumn, IDropdownOption, initializeIcons, IObjectWithKey, Selection, SelectionMode, TextField } from "@fluentui/react";

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
    lineString: string,
    parentId: number
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
    },
    {
        key: 6,
        text: "Array"
    }
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
    const [,setSelectedArrayRow] = useState<IObjectWithKey[]>();

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
        populateServerConfig(setServerConfig, 'ServerConfig/GetServerConfig');
    };

    const handleSave = () => {
        postServerConfig('ServerConfig/PostServerConfig', JSON.stringify(serverConfig))
    };

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
            onRender: (item: StringRow) => {
                return (
                    <TextField
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
            onRender: (item: PropertyValue) => {
                return (
                    <TextField
                        key="propertyName"
                        defaultValue={item.propertyName}
                        onBlur={(event) => handleFieldBlur(event, item, "propertyName")}
                    />
                )
            }
        },
        {
            key: 'dataType',
            fieldName: 'dataType',
            name: 'Data Type',
            minWidth: 160,
            maxWidth: 160,
            onRender: (item) => {
                return (
                    <Dropdown
                        selectedKey={item.dataType}
                        options={dataTypeDropdownOptions}
                        onChange={(_, option) => handleDataTypeSelect(option, item)}
                    />
                )
            }
        },
        {
            key: 'value',
            fieldName: 'value',
            name: 'Value',
            minWidth: 160,
            maxWidth: 320,
            onRender: (item: PropertyValue) => {
                if (item.dataType != DataType.Boolean) {
                    return (
                        <TextField
                            defaultValue={item.value?.toString()}
                            onBlur={(event) => handleValueFieldBlur(event, item)}
                        />
                    )
                }
                else {
                    return(
                        <Dropdown
                            selectedKey={item.value?.toString()}
                            options={booleanDropdown}
                            onChange={(_, option) => option && handleBooleanSelect(option, item)}
                        />
                    )
                }
            }
        },
        {
            key: 'comment',
            fieldName: 'comment',
            name: 'Comment',
            minWidth: 320,
            onRender: (item: PropertyValue) => {
                return (
                    <TextField
                        defaultValue={item.comment}
                        onBlur={(event) => handleFieldBlur(event, item, "comment") }
                    />
                )
            }
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
        if (serverConfig != null) {
            setServerConfig(
                {
                    ...serverConfig,
                    properties:
                        serverConfig.properties.map((property) => {
                            if (property.id == deletedRow.parentId) {
                                return {
                                    ...property,
                                    value: (property.value as string[]).filter((_, index) => index != deletedRow.id)
                                }
                            }
                            else {
                                return property;
                            }
                        })
                }
            );
        }
    };

    const handleArrayFieldBlur = (event: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement, Element>, item: StringRow) => {
        if (serverConfig) {
            const updatedProperties = serverConfig.properties.map(property => {
                if (property.id !== item.parentId) return property;

                const updatedValue = [...(property.value as string[])];
                updatedValue[item.id] = event.target.value;

                return { ...property, value: updatedValue };
            });

            setServerConfig({ ...serverConfig, properties: updatedProperties });
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

    const handleAddArrayItem = (propertyId: number) => {
        if (serverConfig != null) {
            setServerConfig(
                {
                    ...serverConfig,
                    properties: serverConfig.properties.map((property) => {
                        if (property.id == propertyId) {
                            return {
                                ...property,
                                value: [
                                    ...(property.value as string[]),
                                    ""
                                ]
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

    const contents = serverConfig === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        :
        <div>
            <DefaultButton
                onClick={() => handleAddProperty()}
                className="Button"
                style={{ margin: "0px 10px 10px 0px" }}
            >
                <AddIcon/>
            </DefaultButton>
            <DetailsList
                styles={{ root: { display: "flex" } }}
                items={serverConfig.properties}
                columns={columns}
                selectionMode={SelectionMode.none}
            />
            {
                serverConfig.properties.map((property) => {
                    if (property.dataType == DataType.Array) {
                        return (
                            <>
                                <h3>{(property.propertyName == "motd[]") ? "Motto of the day" : property.propertyName}</h3>
                                <DefaultButton
                                    onClick={() => handleAddArrayItem(property.id)}
                                    className="Button"
                                    style={{ margin: "0px 10px 10px 0px" }}
                                >
                                    <AddIcon/>
                                </DefaultButton>
                                <DetailsList
                                    items={(property.value as string[]).map((line, index) => ({ id: index, lineNumber: "Line " + (index + 1), lineString: line, parentId: property.id }))}
                                    columns={arrayColumns}
                                    selection={arraySelection}
                                />
                            </>
                        )
                    }
                })
            }
        </div>

    return (
        <div style={{padding: "10px 10px 10px 10px"} }>
            <h1 id="tableLabel">Server Configurations</h1>
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
    )
}

async function populateServerConfig(setServerConfig: Function, endpoint: string) {
    const response = await fetch(endpoint);
    const result = (await response.json()) as ServerConfig;
    if (result != null) {
        setServerConfig(result);
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