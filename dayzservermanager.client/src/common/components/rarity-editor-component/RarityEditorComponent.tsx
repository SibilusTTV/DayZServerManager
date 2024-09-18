import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Paper, TextField } from "@mui/material";
import { DataGrid, GridActionsCellItem, GridColDef, GridRenderCellParams, GridRowSelectionModel, GridRowsProp, GridSlots, GridToolbarContainer } from '@mui/x-data-grid';
import { ChangeEvent, useEffect, useState } from "react";
import { Dict } from "styled-components/dist/types";
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import "./RarityEditorComponent.css";

interface RarityFile {
    itemRarity: RarityItem[];
}

interface RarityItem {
    id: number,
    name: string,
    rarity: number
}

interface RarityEditorProps {
    name: string;
}

const rarityDefinitions: string[] = [
    "Empty",
    "Poor",
    "Common",
    "Uncommon",
    "Rare",
    "Epic",
    "Legendary",
    "Mythic",
    "Exotic"
]

interface EditToolbarProps {
    rarities: RarityFile;
    checkedItems: number[];
    setRarities: Function;
}

function EditToolbar(props: EditToolbarProps) {
    const { rarities, checkedItems, setRarities } = props;

    const handleClick = () => {
        setRarities(
            {
                ...rarities,
                itemRarity: [
                    {
                        id: getNewId(rarities.itemRarity),
                        name: "",
                        rarity: 0
                    },
                    ...rarities.itemRarity
                ]
            }
        );
    };

    const handleBulkChangeClick = (_value: number) => {
        if (rarities != null) {
            const editedItems: RarityItem[] = [...rarities.itemRarity];
            editedItems.forEach(val => {
                if (checkedItems.includes(val.id)) {
                    val.rarity = _value;
                }
            });
            setRarities(
                {
                    ...rarities,
                    itemRarity: editedItems
                }
            )
        }
    }

    const rarityButtons: JSX.Element[] = new Array;
    rarityDefinitions.forEach((value, index) => {
        rarityButtons.push(
            <Button key={value} id={value} onClick={() => { handleBulkChangeClick(index) }}>{value} ({String(index)})</Button>
        )
    });

    return (
        <GridToolbarContainer>
            <Button color="primary" startIcon={<AddIcon />} onClick={handleClick}>
                Add Rarity
            </Button>
            {rarityButtons.map(x => { return x; })}
        </GridToolbarContainer>
    );
}

const getNewId = (array: RarityItem[]) => {
    let newId: number = 0;
    let idArray: number[] = [];
    array.map((item) => { idArray.push(item.id) })

    for (let i: number = 0; i < idArray.length; i++) {
        if (idArray.find(id => id === i) === undefined) {
            return i;
        }
        else {
            newId++;
        }
    }
    return newId;
}

export default function RarityEditor(props: RarityEditorProps) {
    const [rarities, setRarities] = useState<RarityFile>();
    const [checkedItems, setCheckedItems] = useState<number[]>([])

    useEffect(() => {
        PopulateRarityFile();
    }, []);

    const processRowUpdate = (newRow: RarityItem) => {
        const updatedRow = { ...newRow};
        if (rarities) {
            setRarities({
                itemRarity: rarities.itemRarity.map((row) => (row.name === newRow.name ? updatedRow : row))
            }
            );
        }
        return updatedRow;
    };

    const columns: GridColDef[] = [
        {
            field: 'name',
            headerName: 'Name',
            width: 360,
            editable: true
        },
        {
            field: 'rarity',
            headerName: 'Rarity',
            width: 120,
            type: 'number',
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
                let row: RarityItem = params.row;
                return (
                    <GridActionsCellItem
                        icon={<DeleteIcon />}
                        label="Delete"
                        onClick={() => deleteRarity(row.id)}
                        color="inherit"
                    />
                )
            }
        }
    ];

    const deleteRarity = (id: number) => {
        if (rarities != null) {
            setRarities(
                {
                    ...rarities,
                    itemRarity: rarities.itemRarity.filter(x => x.id != id)
                }
            );
        }
    }

    const onSelectModelChange = (rowSelectionModel: GridRowSelectionModel) => {
        if (rarities) {
            setCheckedItems(
                rowSelectionModel.map((rowId) => {
                    return Number(rowId.valueOf());
                })
            )
        }
    }

    const contents = rarities == null
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        :
        <Paper>
            <DataGrid
                rows={rarities.itemRarity}
                columns={columns}
                initialState={{
                    pagination: {
                        paginationModel: {
                            pageSize: 20,
                        },
                    }
                }}
                pageSizeOptions={[5, 10, 20, 50, 100]}
                checkboxSelection
                sx={{ border: 0 }}
                onRowSelectionModelChange={onSelectModelChange}
                processRowUpdate={processRowUpdate}
                slots={{
                    toolbar: EditToolbar as GridSlots['toolbar'],
                }}
                slotProps={{
                    toolbar: { rarities, checkedItems, setRarities },
                }}
            />
        </Paper>


    return (
        <div>
            <h1 id="tableLabel">{props.name}</h1>
            <div id="SaveButton">
                <Button
                    onClick={PostRarityFile}
                >
                    Save Rarity File!
                </Button>
            </div>
            <div id="Contents">
                {contents}
            </div>
        </div>
    )

    async function PopulateRarityFile() {
        const response = await fetch('/RarityEditor/GetRarityFile/' + props.name);
        const result = (await response.json()) as RarityFile;
        if (result != null) {
            setRarities(result);
        }
    }

    async function PostRarityFile() {
        const response = await fetch('/RarityEditor/PostRarityFile/' + props.name, {
            method: "POST",
            mode: "cors",
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(rarities)
        });
        const result = await response.text();
        if (result != null) {
            alert(result);
        }
    }
}