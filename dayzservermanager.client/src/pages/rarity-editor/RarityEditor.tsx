import { Box, Button } from "@mui/material";
import { Link, Outlet } from "react-router-dom";
import "./RarityEditor.css"

export default function RarityEditor() {
    return (
        <>
            <Box
                component="form"
                sx={{
                    '& .MuiTextField-root': { m: 1, width: '25ch' },
                }}
                noValidate
                autoComplete="off"
                className="RarityEditor-Box"
            >
                <Button color="inherit"><Link to="/rarity-editor/vanilla-rarities-editor">Vanilla Rarities Editor</Link></Button>
                <Button color="inherit"><Link to="/rarity-editor/custom-files-rarities-editor">Custom Files Rarities Editor</Link></Button>
                <Button color="inherit"><Link to="/rarity-editor/expansion-rarities-editor">Expansion Rarities Editor</Link></Button>
            </Box>
            <Outlet />
        </>
    )
}