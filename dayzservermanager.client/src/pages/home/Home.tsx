import { Button, DialogActions, DialogContent, DialogContentText, TextField } from "@mui/material";
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import { GridColDef, GridRenderCellParams, GridActionsCellItem, DataGrid, GridRowSelectionModel } from "@mui/x-data-grid";
import { useEffect, useState } from "react";
import GavelIcon from '@mui/icons-material/Gavel';
import DoNotStepIcon from '@mui/icons-material/DoNotStep';
import "./Home.css";


interface ServerInfo{
    managerStatus: string;
    dayzServerStatus: string;
    steamCMDStatus: string;
    playersCount: number;
    players: Player[];
    adminLog: string;
}

interface Player {
    name: string;
    guid: string;
    id: number;
    ping: number;
    isVerified: boolean;
    isInLobby: boolean;
    ip: string;
}

export default function Home() {
    const [openDialog, setOpenDialog] = useState(false);
    const [steamGuardCode, setSteamGuardCode] = useState("");
    const [serverStatus, setServerStatus] = useState<ServerInfo>();
    const [dialogTimeout, setDialogTimeout] = useState(0);
    const [message, setMessage] = useState("");
    const [selectedPlayer, setSelectedPlayer] = useState(-1);

    useEffect(() => {
        const timer = setInterval(() => {
            if (!openDialog) {
                if (dialogTimeout > 0) {
                    setDialogTimeout(dialogTimeout - 1);
                }
                getServerStatus(setOpenDialog, setServerStatus, dialogTimeout, openDialog, setDialogTimeout);
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

    const handleSendMessage = () => {
        let command = "say";
        command += " " + selectedPlayer + " " + message;
        sendCommand(command);
        setMessage("");
    }

    const KickPlayer = (id: number, name: string) => {
        let reason = prompt("Please give a reason for the kick");
        if (reason == null) {
            reason = "";
        }
        sendKickRequest(id, name, reason);
    }

    const BanPlayer = (id: number, name: string) => {
        let reason = prompt("Please give a reason for the ban");
        if (reason == null) {
            reason = "";
        }
        let durationString = prompt("Please give a duration for the ban or leave it empty for a permanent ban");
        let duration = -1;
        if (durationString != null && durationString != "") {
            duration = GetMinutes(durationString);
        }
        sendBanRequest(id, name, duration, reason);
    }

    const onSelectModelChange = (rowSelectionModel: GridRowSelectionModel) => {
        let rowId = rowSelectionModel.at(0)
        if (rowId != null && serverStatus) {
            let player = serverStatus.players.at(rowId.valueOf() as number)
            if (player) {
                setSelectedPlayer(
                    player.id
                );
            }
            else {
                setSelectedPlayer(-1);
            }
        }
        else {
            setSelectedPlayer(-1);
        }
    }

    const GetMinutes = (durationString: string): number => {
        let pattern = /([0-9]+)[^\S\n]*([Yy]ears?|[Yy]|[Mm]onths?|[Dd]ays?|[Dd]|[Hh]ours?|[Hh]|[Mm]inutes?|[Mm]ins?|[Mm]|)/gm
        let matches = durationString.matchAll(pattern);
        let finalvalue = 0;
        for (let match of matches) {
            let number = parseInt(match[1].valueOf());
            let type = match[2].valueOf();
            switch (type) {
                case "Year":
                case "Years":
                case "year":
                case "years":
                case "Y":
                case "y":
                    finalvalue += number * 365 * 24 * 60;
                    break;
                case "Month":
                case "Months":
                case "month":
                case "months":
                    finalvalue += number * 30 * 24 * 60;
                    break;
                case "Day":
                case "Days":
                case "day":
                case "days":
                case "D":
                case "d":
                    finalvalue += number * 24 * 60;
                    break;
                case "Hour":
                case "Hours":
                case "hour":
                case "hours":
                case "H":
                case "h":
                    finalvalue += number * 60;
                    break;
                case "Minute":
                case "Minutes":
                case "minute":
                case "minutes":
                case "Min":
                case "Mins":
                case "min":
                case "mins":
                case "M":
                case "m":
                default:
                    finalvalue += number;
                    break;

            }
        }
        return finalvalue;
    }

    const columns: GridColDef[] = [
        {
            field: 'name',
            headerName: 'Name',
            width: 360,
            type: 'string',
            editable: false
        },
        {
            field: 'guid',
            headerName: 'Guid',
            width: 360,
            type: 'string',
            editable: false
        },
        {
            field: 'id',
            headerName: 'Id',
            width: 80,
            type: 'number',
            editable: false
        },
        {
            field: 'ping',
            headerName: 'Ping',
            width: 80,
            type: 'number',
            editable: false
        },
        {
            field: 'isVerified',
            headerName: 'Is verified',
            width: 80,
            type: 'boolean',
            editable: false
        },
        {
            field: 'isInLobby',
            headerName: 'Is in lobby',
            width: 80,
            type: 'boolean',
            editable: false
        },
        {
            field: 'ip',
            headerName: 'IP',
            width: 360,
            type: 'string',
            editable: false
        },
        {
            field: 'kick',
            headerName: 'Kick',
            width: 80,
            editable: false,
            display: 'flex',
            disableReorder: true,
            filterable: false,
            hideSortIcons: true,
            resizable: false,
            renderCell: (params: GridRenderCellParams<any, Date>) => {
                let row: Player = params.row;
                return (
                    <GridActionsCellItem
                        icon={<DoNotStepIcon />}
                        label="Kick"
                        onClick={() => KickPlayer(row.id, row.name)}
                        color="inherit"
                    />
                );
            }
        },
        {
            field: 'ban',
            headerName: 'Ban',
            width: 80,
            editable: false,
            display: 'flex',
            disableReorder: true,
            filterable: false,
            hideSortIcons: true,
            resizable: false,
            renderCell: (params: GridRenderCellParams<any, Date>) => {
                let row: Player = params.row;
                return (
                    <GridActionsCellItem
                        icon={<GavelIcon />}
                        label="Ban"
                        onClick={() => BanPlayer(row.id, row.name)}
                        color="inherit"
                    />
                );
            }
        }
    ];

    return (
        <div className="HomeContainer">
            <div className="ButtonContainer">
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
            </div>
            {serverStatus &&
            <>
                <h2>Statuses</h2>
                <p className="Home-Status">Manager Status: {serverStatus.managerStatus}</p>
                <p className="Home-Status">DayZ Server Status: {serverStatus.dayzServerStatus}</p>
                <p className="Home-Status">SteamCMD Status: {serverStatus.steamCMDStatus}</p>
                <p className="Home-Status">Players Count: {serverStatus.playersCount}</p>
                {serverStatus.players.length > 0 &&
                    <>
                        <h2>Playerlist</h2>
                        <DataGrid
                            rows={serverStatus.players}
                            columns={columns}
                            initialState={{
                                pagination: {
                                    paginationModel: {
                                        pageSize: 20,
                                    },
                                }
                        }}
                        checkboxSelection
                            disableMultipleRowSelection
                            onRowSelectionModelChange={onSelectModelChange}
                            pageSizeOptions={[5, 10, 20, 50, 100]}
                            sx={{ border: 0 }}
                        />
                    </>
                }
                <h4>Send Message</h4>
                <p className="Home-Message">
                    <TextField
                        className="Home-Message-TextField"
                        value={message}
                        onChange={(event) => setMessage(event.target.value)}
                        onKeyDown={(event) => event.key === "Enter" && handleSendMessage()}
                        label="Send Message to selected player or to everyone"
                    />
                    <Button
                        onClick={handleSendMessage}
                    >
                        Send Message
                    </Button>
                </p>
                <h2>Admin Log</h2>
                <TextField
                    id="outlined-multiline"
                    multiline
                    rows={10}
                    value={serverStatus.adminLog}
                    fullWidth={true}
                    inputProps={{
                        input: {
                            readOnly: true,
                        },
                    }}

                />
            </>}
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

async function getServerStatus(setOpen: Function, setServerStatus: Function, dialogTimeout: number, open: boolean, setDialogTimeout: Function) {
    try {
        const response = await fetch('DayZServer/GetServerStatus');
        if (response.ok) {
            const result = (await response.json()) as ServerInfo;
            setServerStatus(result);
            if (dialogTimeout <= 0 && !open && result.steamCMDStatus === "Steam Guard") {
                setOpen(true);
            }
        }
        else {
            setDialogTimeout(30);
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

async function sendKickRequest(_id: number, _name: string, _reason: string) {
    await fetch('DayZServer/KickPlayer', {
        method: "POST",
        mode: "cors",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            id: _id,
            name: _name,
            duration: 0,
            reason: _reason
        })
    });
}

async function sendBanRequest(_id: number, _name: string, _duration:number, _reason: string) {
    await fetch('DayZServer/BanPlayer', {
        method: "POST",
        mode: "cors",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            id: _id,
            name: _name,
            duration: _duration,
            reason: _reason
        })
    });
}

async function sendCommand(command: string) {
    await fetch('DayZServer/SendCommand', {
        method: "POST",
        mode: "cors",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            value: command
        })
    });
}