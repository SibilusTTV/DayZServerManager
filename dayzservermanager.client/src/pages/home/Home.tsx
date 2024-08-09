import { Button, DialogActions, DialogContent, DialogContentText, TextField } from "@mui/material";
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import { useState } from "react";



export default function Home() {
    const [open, setOpen] = useState(false);
    const [code, setCode] = useState("");

    const handleClose = () => {
        setOpen(false);
        stopDayZServer();
    };

    return (
        <div>
            <Button
                onClick={() => { startDayZServer(setOpen) }}
            >
                Start the Server
            </Button>
            <Button
                onClick={stopDayZServer}
            >
                Stop the Server
            </Button>
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

async function startDayZServer(setOpen: Function) {
    const response = await fetch('DayZServer/StartServer');
    const result = await response.text()
    if (result === "Steam Guard") {
        setOpen(true);
    }
    else {
        alert(result);
    }
}

async function stopDayZServer() {
    const response = await fetch('DayZServer/StopServer');
    const result = await response.text();
    if (result.toLocaleLowerCase() === "true") {
        alert("The server was stopped");
    }
    else {
        alert("The server couldn't be stopped")
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