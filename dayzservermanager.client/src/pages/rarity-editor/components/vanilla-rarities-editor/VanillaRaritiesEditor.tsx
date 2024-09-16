import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, TextField } from "@mui/material";
import { ChangeEvent, useEffect, useState } from "react";
import { Dict } from "styled-components/dist/types";

interface RarityFile {
    itemRarity: Dict;
}

export default function VanillaRaritiesEditor() {
    const [vanillaRarities, setVanillaRarities] = useState<RarityFile>();
    const [openDialog, setOpenDialog] = useState(false);
    const [newRarity, setNewRarity] = useState("");

    useEffect(() => {
        PopulateRarityFile();
    }, []);

    const handleClose = () => {
        setOpenDialog(false);
        setNewRarity("");
    }

    const handleSubmit = () => {
        if (vanillaRarities != null) {
            setVanillaRarities(
                {
                    ...vanillaRarities,
                    itemRarity: {
                        ...vanillaRarities.itemRarity,
                        [newRarity]: 0
                    }
                }
            )
        }
        setNewRarity("");
    }

    const onRarityChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        if (newRarity != null) {
            setNewRarity(String(event.target.value));
        }
    }

    const onChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        if (vanillaRarities != null) {
            setVanillaRarities(
                {
                    ...vanillaRarities,
                    itemRarity: {
                        ...vanillaRarities.itemRarity,
                        [event.target.id]: Number(event.target.value)
                    }
                }
            );
        }
    }

    const AddNewRarity = () => {
        setOpenDialog(true);
    }

    let texts: JSX.Element[] = new Array;
    if (vanillaRarities?.itemRarity != null) {
        for (let key in vanillaRarities.itemRarity) {
            texts.push(
                <div id={key}>
                    {String(key)} <TextField id={key} value={vanillaRarities?.itemRarity[key]} onChange={(event) => {onChange(event)}} /> <Button>Delete</Button>
                </div>
            )
        }
    }

    const contents = vanillaRarities == null
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        :
        <>
            {texts != null && texts.map(x => { return x; })}
        </>

    return (
        <div>
            <h1 id="tableLabel">Rarity Files</h1>
            <Dialog
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
                        onChange={onRarityChange}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button type="submit">Submit</Button>
                </DialogActions>
            </Dialog>
            <div>
                <Button
                    onClick={PostRarityFile}
                >
                    Save Rarity File!
                </Button>
            </div>
            <div>
                {contents}
            </div>
            <div>
                <Button
                    onClick={AddNewRarity}
                >
                    Add new Rarity!
                </Button>
            </div>
        </div>
    )

    async function PopulateRarityFile() {
        const response = await fetch('/RarityEditor/GetRarityFile/vanillaRarities.json');
        const result = (await response.json()) as RarityFile;
        if (result != null) {
            setVanillaRarities(result);
        }
    }

    async function PostRarityFile() {
        const response = await fetch('/RarityEditor/PostRarityFile/vanillaRarities.json', {
            method: "POST",
            mode: "cors",
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(vanillaRarities)
        });
        const result = await response.text();
        if (result != null) {
            alert(result);
        }
    }
}