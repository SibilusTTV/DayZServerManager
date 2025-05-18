import { TextField } from "@fluentui/react"
import { useEffect, useState } from "react";


export default function ManagerLog() {
    const [managerLog, setManagerLog] = useState("");

    useEffect(() => {
        const timer = setInterval(() => {
            getManagerLog(setManagerLog);
        }, 1000)
        return () => clearInterval(timer);
    })

    return (
        <TextField 
            multiline
            value={managerLog}
        />
    )
}

async function getManagerLog(setManagerLog: Function) {
    try {
        const response = await fetch('Manager/GetManagerLog');
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