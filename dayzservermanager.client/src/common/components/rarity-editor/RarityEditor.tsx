import { Button, Checkbox, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, TextField } from "@mui/material";
import { ChangeEvent, useEffect, useState } from "react";
import { Dict } from "styled-components/dist/types";
import { MarqueeSelection } from '@fluentui/react/lib/MarqueeSelection';
import { DetailsList, IColumn, Selection } from '@fluentui/react/lib/DetailsList';
import "./RarityEditor.css";

interface RarityFile {
    itemRarity: Dict;
}

interface RarityEditorProps {
    name: string;
}

const rarityDefinitions: Dict = {
    "Poor": 1,
    "Common": 2,
    "Uncommon": 3,
    "Rare": 4,
    "Epic": 5,
    "Legendary": 6,
    "Mythic": 7,
    "Exotic": 8
}

export default function RarityEditor(props: RarityEditorProps) {
    const [rarities, setRarities] = useState<RarityFile>();
    const [openDialog, setOpenDialog] = useState(false);
    const [newRarityName, setNewRarityName] = useState("");
    const [checkedItems, setCheckedItems] = useState<string[]>([])

    useEffect(() => {
        PopulateRarityFile();
    }, []);

    const handleClose = () => {
        setOpenDialog(false);
        setNewRarityName("");
    }

    const onCheckedChanged = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (event.target.checked) {
            setCheckedItems([
                ...checkedItems,
                event.target.id
            ]
            );
        }
        else {
            setCheckedItems(
                checkedItems.filter(value => value !== event.target.id)
            );
        }
    }

    const deleteRarity = (_key: string) => {
        if (rarities != null) {
            const filteredRarities: Dict = {};
            for (let key in rarities.itemRarity) {
                if (key !== _key) {
                    filteredRarities[key] = rarities.itemRarity[key];
                }
            }
            setRarities(
                {
                    ...rarities,
                    itemRarity: filteredRarities
                }
            );
            setCheckedItems(
                checkedItems.filter(x => x !== _key)
            );
        }
    }

    const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        if (rarities != null) {
            if (!(newRarityName in rarities.itemRarity)) {
                setRarities(
                    {
                        ...rarities,
                        itemRarity: {
                            ...rarities.itemRarity,
                            [newRarityName]: 0
                        }
                    }
                );
            }
        }
        setNewRarityName("");
        setOpenDialog(false);
    }

    const onNewRarityNameChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        if (newRarityName != null) {
            setNewRarityName(String(event.target.value));
        }
    }

    const onChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        if (rarities != null) {
            setRarities(
                {
                    ...rarities,
                    itemRarity: {
                        ...rarities.itemRarity,
                        [event.target.id]: Number(event.target.value)
                    }
                }
            );
        }
    }

    const AddNewRarity = () => {
        setOpenDialog(true);
    }

    const handleBulkChangeClick = (_value: number) => {
        if (rarities != null) {
            const filteredItems: Dict = {};
            for (let checkedItem of checkedItems) {
                filteredItems[checkedItem] = _value;
            }
            setRarities(
                {
                    ...rarities,
                    itemRarity: {
                        ...rarities.itemRarity,
                        ...filteredItems
                    }
                }
            )
        }
    }

    const columns: IColumn[] = [
        {
            key: 'Column0',
            name: 'Name',
            ariaLabel: 'Name',
            fieldName: 'Name',
            minWidth: 360,
            maxWidth: 800,
            isRowHeader: true,
            isResizable: true,
            data: 'string',
            onColumnClick: () => { },
            onRender: (item: [string, number]) => { return <span>{item[0]}</span>; }
        },
        {
            key: 'Column1',
            name: 'Rarity',
            ariaLabel: 'Rarity',
            fieldName: 'Rarity',
            minWidth: 40,
            maxWidth: 800,
            isRowHeader: true,
            isResizable: true,
            data: 'number',
            onColumnClick: () => { },
            onRender: (item: [string, number]) => { return <span>{item[1]}</span>; }
        }
    ];

    const _selection = new Selection();
    

    const contents = rarities == null
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        :
        <div>
            <MarqueeSelection selection={_selection}>
                <DetailsList
                    setKey="items"
                    items={Object.entries(rarities.itemRarity)}
                    columns={columns}
                    selection={_selection}
                    selectionPreservedOnEmptyClick={true}
                    ariaLabelForSelectionColumn="Toggle selection"
                    ariaLabelForSelectAllCheckbox="Toggle selection for all items"
                    checkButtonAriaLabel="select row"
                />
            </MarqueeSelection>
        </div>

    const rarityButtons: JSX.Element[] = new Array;
    for (let key in rarityDefinitions) {
        rarityButtons.push(
            <Button key={key} id={rarityDefinitions[key]} onClick={() => { handleBulkChangeClick(rarityDefinitions[key]) }}>{key}({String(rarityDefinitions[key])})</Button>
        )
    }

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
            <div id="AddNewRarityButton">
                <Button
                    onClick={AddNewRarity}
                >
                    Add new Rarity!
                </Button>
            </div>
            <div id="RarityButtons">
                {rarityButtons.map(x => { return x; })}
            </div>
            <div id="Contents">
                {contents}
            </div>
            <Dialog
                key="Dialog"
                open={openDialog}
                onClose={handleClose}
                PaperProps={{
                    component: 'form',
                    onSubmit: handleSubmit
                }}
            >
                <DialogTitle>Add new Rarity!</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        Add new Rarity!
                    </DialogContentText>
                    <TextField
                        autoFocus
                        required
                        margin="dense"
                        id="name"
                        label="Add new Rarity!"
                        fullWidth
                        variant="standard"
                        onChange={onNewRarityNameChange}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button type="submit">Submit</Button>
                </DialogActions>
            </Dialog>
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