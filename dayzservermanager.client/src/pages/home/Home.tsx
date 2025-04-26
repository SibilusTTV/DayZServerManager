import { Button, DialogActions, DialogContent, DialogContentText, TextField } from "@mui/material";
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import { useEffect, useState } from "react";

interface ServerInfo{
    managerStatus: string;
    dayzServerStatus: string;
    steamCMDStatus: string;
    playersCount: number;
    players: string[];
}

export default function Home() {
    const [openDialog, setOpenDialog] = useState(false);
    const [steamGuardCode, setSteamGuardCode] = useState("");
    const [serverStatus, setServerStatus] = useState<ServerInfo>();
    const [dialogTimeout, setDialogTimeout] = useState(0);

    useEffect(() => {
        const timer = setInterval(() => {
            if (!openDialog) {
                if (dialogTimeout > 0) {
                    setDialogTimeout(dialogTimeout - 1);
                }
                getServerStatus(setOpenDialog, setServerStatus, dialogTimeout, openDialog);
            }
        }, 1000);
        return () => clearInterval(timer);
    })

    const handleClose = () => {
        setOpenDialog(false);
        setDialogTimeout(30);
    };

    const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setOpenDialog(false);
        setDialogTimeout(30);
        sendSteamGuard(steamGuardCode);
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
            {serverStatus && Object.entries(serverStatus!).map(([key, value]) => (key == "players") ? (value as string[]).map(value => (<p>{value}</p>)) : (<p>{key}: {String(value)}</p>))}
            <Dialog
                open={openDialog}
                onClose={handleClose}
                PaperProps={{
                    component: 'form',
                    onSubmit: handleSubmit
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
                        onChange={(event) => { setSteamGuardCode(event.target.value) }}
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

async function getServerStatus(setOpen: Function, setServerStatus: Function, dialogTimeout: number, open: boolean) {
    try {
        const response = await fetch('DayZServer/GetServerStatus');
        const result = (await response.json()) as ServerInfo;
        setServerStatus(result);
        if (dialogTimeout <= 0 && !open && result.steamCMDStatus === "Steam Guard") {
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
            value: _code
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