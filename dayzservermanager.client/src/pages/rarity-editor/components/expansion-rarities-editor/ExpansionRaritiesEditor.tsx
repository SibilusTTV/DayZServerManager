import { Button, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { Dict } from "styled-components/dist/types";

interface RarityFile {
    itemRarity: Dict
}


export default function ExpansionRaritiesEditor() {
    const [expansionRarities, setExpansionRarities] = useState<RarityFile>();

    useEffect(() => {
        PopulateRarityFile();
    }, []);

    let texts: JSX.Element[] = new Array;
    if (expansionRarities?.itemRarity != null) {
        for (let key in expansionRarities.itemRarity) {
            texts.push(
                <div id={key}>
                    {String(key)} <TextField value={expansionRarities?.itemRarity[key]} /> <Button>Delete</Button>
                </div>
            )
        }
    }

    const contents = expansionRarities == null
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        :
        <>
            {texts != null && texts.map(x => { return x; })}
        </>

    return (
        <div>
            <h1 id="tableLabel">Rarity Files</h1>
            <div>
                <Button
                    onClick={PostRarityFile}
                >
                    Save Config!
                </Button>
            </div>
            <div>
                {contents}
            </div>
            <div>
                <Button
                    onClick={AddNewRarity}
                >
                    New Rarity
                </Button>
            </div>
        </div>
    )

    async function PopulateRarityFile() {
        const response = await fetch('/RarityEditor/GetRarityFile/expansionRarities.json');
        const result = (await response.json()) as RarityFile;
        if (result != null) {
            setExpansionRarities(result);
        }
    }

    async function PostRarityFile() {
        const response = await fetch('/RarityEditor/PostRarityFile/expansionRarities.json', {
            method: "POST",
            mode: "cors",
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(expansionRarities)
        });
        const result = await response.text();
        if (result != null) {
            alert(result);
        }
    }

    function AddNewRarity() {

    }
}