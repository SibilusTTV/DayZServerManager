
import { Link, Outlet } from "react-router-dom";
import "./RarityEditor.css"

export default function RarityEditor() {
    return (
        <div style={{ display: "flex", flexDirection: "column" }}>
            <div style={{ display: "flex", flexDirection: "row", margin: "10px 10px 10px 10px", flexGrow: 0  }}
            >
                <Link to="/rarity-editor/vanilla-rarities-editor" className="Layout-Link">Vanilla Rarities Editor</Link>
                <Link to="/rarity-editor/custom-files-rarities-editor" className="Layout-Link">Custom Files Rarities Editor</Link>
                <Link to="/rarity-editor/expansion-rarities-editor" className="Layout-Link">Expansion Rarities Editor</Link>
            </div>
            <Outlet />
        </div>
    )
}