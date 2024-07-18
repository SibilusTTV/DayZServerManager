import { Button } from "@mui/material";


export default function Home() {
    return (
        <div>
            <Button
                onClick={startDayZServer}
            >
                Start the Server
            </Button>
            <Button
                onClick={stopDayZServer}
            >
                Stop the Server
            </Button>
        </div>
    )
}

async function startDayZServer() {
    const response = await fetch('DayZServer/StartServer');
    const result = await response.text()
    alert(result);
}

async function stopDayZServer() {
    const response = await fetch('DayZServer/StopServer');
    const result = await response.text()
    if (result.toLocaleLowerCase() === "true") {
        alert("The server was stopped");
    }
    else {
        alert("The server couldn't be stopped")
    }
}