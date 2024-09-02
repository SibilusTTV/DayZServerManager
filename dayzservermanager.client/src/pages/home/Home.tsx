import { Button, DialogActions, DialogContent, DialogContentText, TextField } from "@mui/material";
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import { useEffect, useState } from "react";


export default function Home() {
    const [open, setOpen] = useState(false);
    const [code, setCode] = useState("");
    const [serverStatus, setServerStatus] = useState("Server not started");

    useEffect(() => {
        setInterval(() => { getServerStatus(setOpen, setServerStatus) }, 1000);
    }, [])

    const handleClose = () => {
        setOpen(false);
        stopDayZServer();
    };

    return (
        <div>
            <Button
                onClick={() => { startDayZServer() }}
            >
                Start the Server
            </Button>
            <Button
                onClick={stopDayZServer}
            >
                Stop the Server
            </Button>
            <Button
                onClick={() => { restartDayZServer() } }
            >
                Restart the Server
            </Button>
            <p>
                {serverStatus}
            </p>
            <Dialog
                open={open}
                onClose={handleClose}
                PaperProps={{
                    component: 'form',
                    onSubmit: (event: React.FormEvent<HTMLFormElement>) => {
                        event.preventDefault();
                        sendSteamGuard(code, setOpen);
                        setOpen(false);
                    },
                }}
            >
                <DialogTitle>Steam Guard</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        To proceed in downloading the Server and the mods, you need to enter your steam guard code.
                    </DialogContentText>
                    <TextField
                        autoFocus
                        required
                        margin="dense"
                        id="name"
                        name="code"
                        label="Steam guard code"
                        type="password"
                        fullWidth
                        variant="standard"
                        onChange={(event) => { setCode(event.target.value) }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>Cancel</Button>
                    <Button type="submit">Submit</Button>
                </DialogActions>
            </Dialog>
        </div>
    )
}

async function getServerStatus(setOpen: Function, setServerStatus: Function) {
    try {
        const response = await fetch('DayZServer/GetServerStatus');
        const result = await response.text()
        setServerStatus(result);
        if (result === "Steam Guard") {
            setOpen(true);
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

async function startDayZServer() {
    try {
        const response = await fetch('DayZServer/StartServer');
        const result = await response.text();
        if (result.toLocaleLowerCase() === "true") {
            alert("Server is starting");
        }
        else {
            alert("The server couldn't be started");
        }
        return result;
    }
    catch (ex) {
        if (typeof ex === "string") {
            alert(ex);
        }
        else if (ex instanceof Error) {
            alert(ex.message);
        }
        return "false";
    }
}

async function stopDayZServer() {
    try {
        const response = await fetch('DayZServer/StopServer');
        const result = await response.text();
        if (result.toLocaleLowerCase() === "true") {
            alert("Server was stopped");
        }
        else {
            alert("The server couldn't be stopped");
        }
        return result;
    }
    catch (ex) {
        if (typeof ex === "string") {
            alert(ex);
        }
        else if (ex instanceof Error) {
            alert(ex.message);
        }
        return "false";
    }
}

async function restartDayZServer() {
    const stopMessage = await stopDayZServer();
    if (stopMessage.toLocaleLowerCase() === "true") {
        startDayZServer();
    }
}

async function sendSteamGuard(_code: string, setOpen: Function) {
    const response = await fetch('DayZServer/SendSteamGuard', {
        method: "POST",
        mode: "cors",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            code: _code
        })
    });
    const result = await response.text();

    if (result == "Steam guard") {
        setOpen(true);
    }
    else {
        alert(result);
    }
}