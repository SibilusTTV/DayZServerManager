import { Button, DialogActions, DialogContent, DialogContentText, TextField } from "@mui/material";
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import { useEffect, useState } from "react";

interface ServerInfo{
    managerStatus: string;
    dayzServerStatus: string;
    steamCMDStatus: string;
    players: number;
}

export default function Home() {
    const [open, setOpen] = useState(false);
    const [code, setCode] = useState("");
    const [serverStatus, setServerStatus] = useState<ServerInfo>();
    const [codeSent, setCodeSent] = useState(false);
    const [countdown, setCountdown] = useState(0);

    useEffect(() => {
        setInterval(() => {
            if (codeSent) {
                if (countdown <= 0) {
                    setCodeSent(false);
                }
                else {
                    setCountdown(countdown - 1);
                }
            }
            if (!open) {
                getServerStatus(setOpen, setServerStatus, codeSent, open);
            }
        }, 1000);
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
                Start Server
            </Button>
            <Button
                onClick={stopDayZServer}
            >
                Stop Server
            </Button>
            <Button
                onClick={() => { restartDayZServer() } }
            >
                Restart Server
            </Button>
            {serverStatus && Object.entries(serverStatus!).map(([key, value]) => (<p>{key}: {String(value)}</p>))}
            <Dialog
                open={open}
                onClose={handleClose}
                PaperProps={{
                    component: 'form',
                    onSubmit: (event: React.FormEvent<HTMLFormElement>) => {
                        event.preventDefault();
                        setCountdown(30);
                        setCodeSent(true);
                        setOpen(false);
                        sendSteamGuard(code);
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

async function getServerStatus(setOpen: Function, setServerStatus: Function, codeSent: boolean, open: boolean) {
    try {
        const response = await fetch('DayZServer/GetServerStatus');
        const result = (await response.json()) as ServerInfo;
        setServerStatus(result);
        if (!codeSent && !open && result.steamCMDStatus === "Steam Guard") {
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

async function sendSteamGuard(_code: string) {
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

    if (result.toLocaleLowerCase() === "true") {
        alert("Steam Guard was sent");
    }
    else {
        alert("Steam Guard couldn't be sent");
    }
}