import { TextField } from "@fluentui/react"
import { useEffect, useState } from "react";


export default function ManagerLog() {
    const [managerLog, setManagerLog] = useState("");

    useEffect(() => {
        const timer = setInterval(() => {
            handleLoad();
        }, 5000)
        return () => clearInterval(timer);
    })

    const handleLoad = () => {
        getManagerLog(setManagerLog);
    }

    return (
        <div style={{ display: "flex", flexDirection: "column", margin: "10px 10px 10px 10px", flexGrow: 0, gap: "10px" }}>
            <TextField
                multiline
                value={managerLog}
            />
        </div>
    )
}

async function getManagerLog(setManagerLog: Function) {
    try {
        const response = await fetch('/Manager/GetManagerLog');
        if (response.status == 200) {
            const result = await response.text();
            setManagerLog(result);
        }
    }
    catch (ex) {
        if (typeof ex === "string") {
            alert(ex);
        }
        else if (ex instanceof Error) {
            alert(ex.message);
        }
    }
}