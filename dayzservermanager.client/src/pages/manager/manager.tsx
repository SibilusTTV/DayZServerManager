
import { Link, Outlet } from "react-router-dom";

export default function Manager() {
    return (
        <div style={{ display: "flex", flexDirection: "column" }}>
            <div style={{ display: "flex", flexDirection: "row", margin: "10px 10px 10px 10px", flexGrow: 0, gap: "10px" }}
            >
                <Link to="/manager/manager-config" className="Layout-Link">Manager Config Editor</Link>
                <Link to="/manager/manager-log" className="Layout-Link">Manager Log</Link>
            </div>
            <Outlet />
        </div>
    )
}