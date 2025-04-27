import { TextField } from "@mui/material";
import { useEffect, useState } from "react";


export default function ManagerLog() {
    const [managerLog, setManagerLog] = useState("");

    useEffect(() => {
        const timer = setInterval(() => {
            getManagerLog(setManagerLog, managerLog);
        }, 1000)
        return () => clearInterval(timer);
    })

    return (
        <TextField style={{ display: "flex", padding: "10px 10px 10px 10px" }}
            multiline
            value={managerLog}
            maxRows="20"
            minRows= "5"
        />
    )
}

async function getManagerLog(setManagerLog: Function, managerLog: string) {
    try {
        const response = await fetch('DayZServer/GetManagerLog');
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