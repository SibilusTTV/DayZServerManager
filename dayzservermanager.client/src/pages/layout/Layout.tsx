
import {Home} from "@mui/icons-material";
import { Link, Outlet } from "react-router-dom";
import "./Layout.css"


export default function Layout() {
    return (
        <div style={{ display: "flex", flexDirection: "column", minWidth: "600px" }}>
            <div style={{ display: "flex", flexDirection: "row", margin: "10px 10px 10px 10px", flexGrow: 0, gap: "10px" }}>
                <Link to="/" className="Layout-Link">
                    <Home style={{flexGrow: 1}} />
                </Link>
                <Link to="/manager" className="Layout-Link">Manager</Link>
                <Link to="/player-database" className="Layout-Link">Player Database</Link>
                <Link to="/server-config" className="Layout-Link">Server Config Editor</Link>
                <Link to="/scheduler-config" className="Layout-Link">Scheduler Config Editor</Link>
                <Link to="/rarity-editor" className="Layout-Link">Rarity Editor</Link>
            </div>
            <Outlet />
        </div>
    )
}