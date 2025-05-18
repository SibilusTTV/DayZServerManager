
import { useEffect, useMemo, useRef, useState } from "react";
import "./Home.css";
import KickButton from "../../common/components/kick-button/KickButton";
import BanButton from "../../common/components/ban-button/BanButton";
import { DefaultButton, DetailsList, IColumn, IObjectWithKey, Selection, SelectionMode, TextField } from "@fluentui/react";

interface ServerInfo{
    managerStatus: string;
    dayzServerStatus: string;
    steamCMDStatus: string;
    playersCount: number;
    players: Player[];
    chatLog: string;
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
    isBanned: boolean;
}

export default function Home() {
    const [serverStatus, setServerStatus] = useState<ServerInfo>();
    const [dialogTimeout, setDialogTimeout] = useState<number>(0);
    const [message, setMessage] = useState("");
    const [selectedPlayers, setSelectedPlayers] = useState<IObjectWithKey[]>();
    const dialogTimeoutRef = useRef(dialogTimeout);

    useEffect(() => {
        dialogTimeoutRef.current = dialogTimeout;
    }, [dialogTimeout]);

    useEffect(() => {
        reload();
        const timer = setInterval(() => {
            reload();
        }, 5000);
        return () => clearInterval(timer);
    }, [])

    const reload = () => {
        setDialogTimeout(prevTimeout => {
            if (prevTimeout > 0) {
                return prevTimeout - 1;
            } else {
                return prevTimeout;
            }
        })
        getServerStatus(setServerStatus, dialogTimeoutRef, setDialogTimeout);
    }

    const handleSendMessage = () => {
        let playerId: number = -1;
        if (selectedPlayers && selectedPlayers.length > 0) {
            playerId = (selectedPlayers[0] as Player).id
        }
        let command = "say";
        command += " " + playerId + " " + message;
        sendCommand(command);
        setMessage("");
    };

    const playerSelection: Selection = useMemo(() => new Selection(
        {
            onSelectionChanged: () => {
                setSelectedPlayers(playerSelection.getSelection());
            },
            selectionMode: SelectionMode.single,
            getKey: (item) => (item as Player).id
        }),
        []
    );

    const columns: IColumn[] = [
        {
            key: 'name',
            fieldName: 'name',
            name: 'Name',
            minWidth: 360
        },
        {
            key: 'guid',
            fieldName: 'guid',
            name: 'Guid',
            minWidth: 360
        },
        {
            key: 'id',
            fieldName: 'id',
            name: 'Id',
            minWidth: 80
        },
        {
            key: 'ping',
            fieldName: 'ping',
            name: 'Ping',
            minWidth: 80
        },
        {
            key: 'isVerified',
            fieldName: 'isVerified',
            name: 'Is verified',
            minWidth: 80
        },
        {
            key: 'isInLobby',
            fieldName: 'isInLobby',
            name: 'Is in lobby',
            minWidth: 80
        },
        {
            key: 'ip',
            fieldName: 'ip',
            name: 'IP',
            minWidth: 360
        },
        {
            key: 'kick',
            fieldName: 'kick',
            name: 'Kick',
            minWidth: 100,
            onRender: (item: Player) => {
                return (
                    <KickButton
                        guid={item.guid}
                        name={item.name}
                        reload={reload}
                    />
                );
            }
        },
        {
            key: 'ban',
            fieldName: 'ban',
            name: 'Ban',
            minWidth: 100,
            onRender: (item: Player) => {
                return (
                    <BanButton
                        guid={item.guid}
                        name={item.name}
                        reload={reload}
                    />
                );
            }
        }
    ];

    return (
        <div className="HomeContainer">
            <div className="ButtonContainer">
                <DefaultButton
                    onClick={() => { startDayZServer() }}
                    className="Button"
                    style={{margin: "0px 10px 0px 0px"}}
                >
                    Start Server
                </DefaultButton>
                <DefaultButton
                    onClick={stopDayZServer}
                    className="Button"
                    style={{ margin: "0px 10px 0px 0px" }}
                >
                    Stop Server
                </DefaultButton>
                <DefaultButton
                    onClick={() => { restartDayZServer() }}
                    className="Button"
                    style={{ margin: "0px 10px 0px 0px" }}
                >
                    Restart Server
                </DefaultButton>
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
                        <DetailsList
                        items={serverStatus.players}
                        columns={columns}
                        selection={playerSelection}
                        />
                    </>
                }
                <h4>Send Message</h4>
                <p className="Home-Message">
                    <TextField
                        className="Home-Message-TextField"
                        value={message}
                        onChange={(_, newValue) => newValue && setMessage(newValue)}
                        onKeyDown={(event) => event.key === "Enter" && handleSendMessage()}
                        label="Send Message to selected player or to everyone"
                    />
                    <DefaultButton
                        onClick={handleSendMessage}
                        className="Button"
                        style={{ margin: "0px 10px 0px 10px" }}
                    >
                        Send Message
                    </DefaultButton>
                </p>
                <h2>Scheduler Log</h2>
                <TextField
                    key="chatLog"
                    multiline
                    rows={10}
                    value={serverStatus.chatLog || ""}
                    readOnly
                />
                <h2>Admin Log</h2>
                <TextField
                    key="adminLog"
                    multiline
                    rows={10}
                    readOnly
                    value={serverStatus.adminLog || ""}
                />
            </>}
        </div>
    )
}

async function getServerStatus(setServerStatus: Function, dialogTimeoutRef: React.RefObject<number>, setDialogTimeout: Function) {
    try {
        const response = await fetch('Manager/GetServerStatus');
        if (response.ok) {
            const result = (await response.json()) as ServerInfo;
            setServerStatus(result);
            if (dialogTimeoutRef.current == 0 && result.steamCMDStatus === "Steam Guard") {
                setDialogTimeout(-1);
                const steamguard = prompt("Please provide the Steam Guard code");
                steamguard && sendSteamGuard(steamguard);
                setDialogTimeout(6);
            }
        }
        else {
            setDialogTimeout(6);
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
        const response = await fetch('Manager/StartServer');
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
        const response = await fetch('Manager/StopServer');
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
    try {
        const stopMessage = await stopDayZServer();
        if (stopMessage.toLocaleLowerCase() === "true") {
            startDayZServer();
        }
    }
    catch (ex) {
        alert(ex);
    }
}

async function sendSteamGuard(_code: string) {
    try {
        const response = await fetch('Manager/SendSteamGuard', {
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
    catch (ex) {
        alert(ex);
    }
}

async function sendCommand(command: string) {
    try {
        await fetch('Scheduler/SendCommand', {
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
    catch (ex) {
        alert(ex);
    }
}