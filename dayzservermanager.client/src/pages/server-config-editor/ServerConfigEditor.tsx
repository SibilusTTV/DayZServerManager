import { useEffect, useState } from "react";
import { Button, MenuItem, Paper, Select, SelectChangeEvent } from '@mui/material';
import { GridColDef, GridRenderCellParams, GridActionsCellItem, DataGrid, GridRowId, GridToolbarContainer } from "@mui/x-data-grid";
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import AddIcon from '@mui/icons-material/Add';



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

interface CustomPropertyToolbarProps {
    serverConfig: ServerConfig | undefined;
    setServerConfig: Function;
}

interface CustomArrayToolbarProps {
    propertyId: number;
    serverConfig: ServerConfig | undefined;
    setServerConfig: Function;
}

function CustomPropertyToolbar({ serverConfig, setServerConfig }: CustomPropertyToolbarProps) {
    const GetNextId = () => {
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
                        { id: GetNextId(), propertyName: "", dataType: DataType.Text, value: "", comment: "" }
                    ]
                }
            )
        }
    }
    return (
        <GridToolbarContainer>
            <Button color="primary" startIcon={<AddIcon />} onClick={handleAddProperty}>
                Add Property
            </Button>
        </GridToolbarContainer>
    )
}

function CustomArrayToolbar({ propertyId, serverConfig, setServerConfig }: CustomArrayToolbarProps) {
    const handleAddArrayItem = () => {
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
    return (
        <GridToolbarContainer>
            <Button color="primary" startIcon={<AddIcon />} onClick={handleAddArrayItem}>
                Add Array Item
            </Button>
        </GridToolbarContainer>
    )
}

export default function ServerConfigEditor() {

    const [serverConfig, setServerConfig] = useState<ServerConfig>();

    useEffect(() => {
        populateServerConfig();
    }, []);

    const arrayColumns: GridColDef[] = [
        {
            field: 'lineNumber',
            headerName: 'Line Number',
            width: 360,
            editable: false
        },
        {
            field: 'lineString',
            headerName: 'Content',
            width: 360,
            editable: true
        },
        {
            field: 'delete',
            headerName: 'Delete',
            width: 80,
            editable: false,
            display: 'flex',
            disableReorder: true,
            filterable: false,
            hideSortIcons: true,
            resizable: false,
            renderCell: (params: GridRenderCellParams<any, Date>) => {
                let row: StringRow = params.row
                return (
                    <GridActionsCellItem
                        icon={<DeleteIcon />}
                        label="Delete"
                        onClick={() => deleteArrayItem(row)}
                        color="inherit"
                    />
                )
            }
        }
    ]

    const columns: GridColDef[] = [
        {
            field: 'propertyName',
            headerName: 'Property Name',
            width: 360,
            editable: true
        },
        {
            field: 'dataType',
            headerName: 'Data Type',
            width: 360,
            type: 'singleSelect',
            editable: false,
            display: 'flex',
            renderCell: (params: GridRenderCellParams<any, Date>) => {
                let row: PropertyValue = params.row;
                return (
                    <Select
                        value={row.dataType}
                        onChange={(event: SelectChangeEvent<DataType>) => handleDataTypeSelect(event, row)}
                    >
                        <MenuItem value={DataType.Number}>Number</MenuItem>
                        <MenuItem value={DataType.Text}>Text (128)</MenuItem>
                        <MenuItem value={DataType.TextLong}>Text Long (256)</MenuItem>
                        <MenuItem value={DataType.TextShort}>Text Short (64)</MenuItem>
                        <MenuItem value={DataType.Decimal}>Decimal</MenuItem>
                        <MenuItem value={DataType.Boolean}>Boolean</MenuItem>
                        <MenuItem value={DataType.Array}>Array</MenuItem>
                    </Select>
                )
            }
        },
        {
            field: 'value',
            headerName: 'Value',
            width: 360,
            type: 'string',
            editable: true
        },
        {
            field: 'comment',
            headerName: 'Comment',
            width: 360,
            type: 'string',
            editable: true
        },
        {
            field: 'delete',
            headerName: 'Delete',
            width: 80,
            editable: false,
            display: 'flex',
            disableReorder: true,
            filterable: false,
            hideSortIcons: true,
            resizable: false,
            renderCell: (params: GridRenderCellParams<any, Date>) => {
                let row: PropertyValue = params.row;
                return (
                    <GridActionsCellItem
                        icon={<DeleteIcon />}
                        label="Delete"
                        onClick={() => deleteProperty(row.id)}
                        color="inherit"
                    />
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

    const handleDataTypeSelect = (event: SelectChangeEvent<DataType>, row: PropertyValue) => {
        if (serverConfig != null) {
            setServerConfig(
                {
                    properties: serverConfig.properties.map((property) => {
                        if (property.id == row.id) {
                            let dataType = event.target.value as DataType;
                            let newValue;
                            switch (dataType) {
                                case DataType.Number:
                                    newValue = parseInt(property.value as string);
                                    break;
                                case DataType.Text:
                                    newValue = (property.value as string).substring(0, 128);
                                    break;
                                case DataType.TextLong:
                                    newValue = (property.value as string).substring(0, 256);
                                    break;
                                case DataType.TextShort:
                                    newValue = (property.value as string).substring(0, 64);
                                    break;
                                case DataType.Decimal:
                                    newValue = parseFloat(property.value as string);
                                    break;
                                case DataType.Boolean:
                                    newValue = booleanify(property.value as string);
                                    break;
                                case DataType.Array:
                                    newValue = property.value;
                                    if (!Array.isArray(newValue)) {
                                        newValue = [""];
                                    }
                                    break;
                            }
                            return {
                                ...property,
                                dataType: dataType,
                                value: newValue
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

    const processRowUpdate = (newRow: PropertyValue, oldRow: PropertyValue, params: { rowId: GridRowId }) => {
        let newValue;
        switch (newRow.dataType) {
            case DataType.Number:
                newValue = parseInt(newRow.value as string);
                break;
            case DataType.Text:
                newValue = (newRow.value as string).substring(0, 128);
                break;
            case DataType.TextLong:
                newValue = (newRow.value as string).substring(0, 256);
                break;
            case DataType.TextShort:
                newValue = (newRow.value as string).substring(0, 64);
                break;
            case DataType.Decimal:
                newValue = parseFloat(newRow.value as string);
                break;
            case DataType.Boolean:
                newValue = booleanify(newRow.value as string);
                break;
            case DataType.Array:
                newValue = oldRow.value;
                if (!Array.isArray(newValue)) {
                    newValue = [""];
                }
                break;
        }
        const updatedRow: PropertyValue = { ...newRow, value: newValue};
        if (serverConfig) {
            setServerConfig(
                {
                    properties: serverConfig.properties.map((row) => (row.propertyName === newRow.propertyName ? updatedRow : row))
                }
            );
        }
        return updatedRow;
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

    const processArrayItemUpdate = (newArrayItem: StringRow, oldArrayItem: StringRow, params: {rowId: GridRowId}) => {
        if (serverConfig != null) {
            setServerConfig(
                {
                    ...serverConfig,
                    properties: 
                        serverConfig.properties.map<PropertyValue>((property) => {
                            if (property.id == newArrayItem.parentId) {
                                return {
                                    ...property,
                                    value: (property.value as string[]).map((line, index) => {
                                        if (index == newArrayItem.id) {
                                            return newArrayItem.lineString;
                                        }
                                        else {
                                            return line;
                                        }
                                    })
                                };
                            }
                            else {
                                return property;
                            }
                        })
                }
            );
        }
        return newArrayItem;
    }

    const contents = serverConfig === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        :
        <Paper>
            <DataGrid
                rows={serverConfig.properties}
                columns={columns}
                initialState={{
                    pagination: {
                        paginationModel: {
                            pageSize: 20,
                        },
                    }
                }}
                pageSizeOptions={[5, 10, 20, 50, 100]}
                sx={{ border: 0 }}
                processRowUpdate={processRowUpdate}
                slots={{
                    toolbar: () => CustomPropertyToolbar({serverConfig: serverConfig, setServerConfig: setServerConfig })
                }}
            />
            {
                serverConfig.properties.map((property) => {
                    if (property.dataType == DataType.Array) {
                        return (
                            <>
                                <h3>{(property.propertyName == "motd[]") ? "Motto of the day" : property.propertyName}</h3>
                                <DataGrid
                                    rows={(property.value as string[]).map((line, index) => ({ id: index, lineNumber: "Line " + (index + 1) ,lineString: line, parentId: property.id }))}
                                    columns={arrayColumns}
                                    initialState={{
                                        pagination: {
                                            paginationModel: {
                                                pageSize: 20,
                                            },
                                        }
                                    }}
                                    pageSizeOptions={[5, 10, 20, 50, 100]}
                                    sx={{ border: 0 }}
                                    processRowUpdate={processArrayItemUpdate}
                                    slots={{
                                        toolbar: () => CustomArrayToolbar({propertyId: property.id, serverConfig: serverConfig, setServerConfig: setServerConfig})
                                    }}
                                />
                            </>
                        )
                    }
                })
            }
        </Paper>

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
        if (result != null) {
            setServerConfig(result);
        }
    }

    async function postServerConfig() {
        const response = await fetch('ServerConfig/PostServerConfig', {
            method: "POST",
            mode: "cors",
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(serverConfig)
        });
        const result = await response.text()
        if (result != null) {
            alert(result);
        }
    }

    async function saveServerConfig() {
        const response = await fetch('ServerConfig/SaveServerConfig');
        const result = await response.text();
        if (result != null) {
            alert(result);
        }
    }


}