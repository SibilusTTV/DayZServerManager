import { useEffect, useState } from 'react';
import './App.css';
import { TextField, Box, Button } from '@mui/material';

interface ManagerConfig {
    steamUsername: string;
    steamPassword: string;
    serverPath: string;
    steamCMDPath: string;
    becPath: string;
    workshopPath: string;
    backupPath: string;
    missionName: string;
    instanceId: number;
    serverConfigName: string;
    profileName: string;
    port: number;
    steamQueryPort: number;
    RConPort: number;
    cpuCount: number;
    noFilePatching: boolean;
    doLogs: boolean;
    adminLog: boolean;
    freezeCheck: boolean;
    netLog: boolean;
    limitFPS: number;
    vanillaMissionName: string;
    missionTemplatePath: string;
    expansionDownloadPath: string;
    mapName: string;
    restartOnUpdate: boolean;
    restartInterval: number;
    autoStartServer: boolean;
    clientMods: Mod[];
    serverMods: Mod[];
}

interface Mod {
    workshopID: number;
    name: string;
}

interface ServerConfig {
    hostname: string;
    password: string;
    passwordAdmin: string;
    enableWhitelist: number;
    disableBanlist: boolean;
}

function App() {
    const [managerConfig, setManagerConfig] = useState<ManagerConfig>();
    const [serverConfig, setServerConfig] = useState<ServerConfig>();

    useEffect(() => {
        populateManagerConfig();
    }, []);

    useEffect(() => {
        populateServerConfig();
    }, []);

    const row2 = serverConfig === undefined ? <p>Nothing to see here</p> :
        <div>
            <TextField id="hostname" variant="outlined" label="Hostname" defaultValue={serverConfig.hostname} />
            <TextField id="password" variant="outlined" label="Password" defaultValue={serverConfig.password} />
            <TextField id="passwordAdmin" variant="outlined" label="Admin Password" defaultValue={serverConfig.passwordAdmin} />
            <TextField id="enableWhitelist" variant="outlined" label="Enable Whitelist" defaultValue={serverConfig.enableWhitelist} />
            <TextField id="disableBanlist" variant="outlined" label="Disable Ban List" defaultValue={serverConfig.disableBanlist} />
        </div>

    const contents = managerConfig === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        :<Box
            component="form"
            sx={{
                '& .MuiTextField-root': { m: 1, width: '25ch' },
            }}
            noValidate
            autoComplete="off"
            >
            <div>
                <TextField id="steamUsername" variant="outlined" label="Steam Username" defaultValue={managerConfig.steamUsername} />
                <TextField id="steamPassword" variant="outlined" label="Steam Password" defaultValue={managerConfig.steamPassword} />
                <TextField id="serverPath" variant="outlined" label="Server Path" defaultValue={managerConfig.serverPath} />
                <TextField id="steamCMDPath" variant="outlined" label="Steam CMD Path" defaultValue={managerConfig.steamCMDPath} />
                <TextField id="becPath" variant="outlined" label="BEC Path" defaultValue={managerConfig.becPath} />
                <TextField id="workshopPath" variant="outlined" label="Workshop Path" defaultValue={managerConfig.workshopPath} />
                <TextField id="backupPath" variant="outlined" label="Backup Path" defaultValue={managerConfig.backupPath} />
                <TextField id="missionName" variant="outlined" label="Mission Name" defaultValue={managerConfig.missionName} />
                <TextField id="instanceId" variant="outlined" label="Instance ID" defaultValue={managerConfig.instanceId} />
                <TextField id="serverConfigName" variant="outlined" label="Server Config Name" defaultValue={managerConfig.serverConfigName} />
                <TextField id="profileName" variant="outlined" label="Profile Name" defaultValue={managerConfig.profileName} />
                <TextField id="port" variant="outlined" label="Port" defaultValue={managerConfig.port} />
                <TextField id="steamQueryPort" variant="outlined" label="Steam Query Port" defaultValue={managerConfig.steamQueryPort} />
                <TextField id="RConPort" variant="outlined" label="RCon Port" defaultValue={managerConfig.RConPort} />
                <TextField id="cpuCount" variant="outlined" label="CPU Count" defaultValue={managerConfig.cpuCount} />
                <TextField id="noFilePatching" variant="outlined" label="No File Patching" defaultValue={managerConfig.noFilePatching} />
                <TextField id="doLogs" variant="outlined" label="Do Logs" defaultValue={managerConfig.doLogs} />
                <TextField id="adminLog" variant="outlined" label="Admin Log" defaultValue={managerConfig.adminLog} />
                <TextField id="freezeCheck" variant="outlined" label="Freeze Check" defaultValue={managerConfig.freezeCheck} />
                <TextField id="netLog" variant="outlined" label="Net Log" defaultValue={managerConfig.netLog} />
                <TextField id="limitFPS" variant="outlined" label="Limit FPS" defaultValue={managerConfig.limitFPS} />
                <TextField id="vanillaMissionName" variant="outlined" label="Vanilla Mission Name" defaultValue={managerConfig.vanillaMissionName} />
                <TextField id="missionTemplatePath" variant="outlined" label="Mission Template Path" defaultValue={managerConfig.missionTemplatePath} />
                <TextField id="expansionDownloadPath" variant="outlined" label="Expansion Download Path" defaultValue={managerConfig.expansionDownloadPath} />
                <TextField id="mapName" variant="outlined" label="mapName" defaultValue={managerConfig.mapName} />
                <TextField id="restartOnUpdate" variant="outlined" label="Restart On Update" defaultValue={managerConfig.restartOnUpdate} />
                <TextField id="restartInterval" variant="outlined" label="Restart Interval" defaultValue={managerConfig.restartInterval} />
                <TextField id="autoStartServer" variant="outlined" label="Autostart Server" defaultValue={managerConfig.autoStartServer} />
            </div>
            <div>
                {managerConfig.clientMods.map((mod) => (
                    <div id={mod.workshopID.toString()}>
                        <TextField id="workshopID" variant="outlined" label="Autostart Server" defaultValue={mod.workshopID} />
                        <TextField id="name" variant="outlined" label="Autostart Server" defaultValue={mod.name} />
                    </div>
                ))}
                <Button
                    onClick={() => { const newMod: Mod = { workshopID: 0, name: ""};  managerConfig.clientMods?.push(newMod) }}
                >
                    Add new Row
                </Button>
            </div>
            <Button
                onClick={async () => {
                    const response = await fetch('DayZServer/StartServer');
                    const result = await response.text()
                    alert(result);
                }}
            >
                Just hit it!
            </Button>
            {row2}
        </Box>

    return (
        <div>
            <h1 id="tableLabel">Configs</h1>
            <p>Some Configs</p>
            {contents}
        </div>
    );

    async function populateManagerConfig() {
        const response = await fetch('ManagerConfig/GetManagerConfig');
        const result = (await response.json()) as ManagerConfig;
        setManagerConfig(result);
    }

    async function populateServerConfig() {
        const response = await fetch('ServerConfig/GetServerConfig');
        const result = (await response.json()) as ServerConfig;
        setServerConfig(result);
    }
    
}

export default App;