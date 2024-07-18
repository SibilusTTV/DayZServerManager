import { useEffect, useState } from "react";
import { TextField } from '@mui/material';



interface ServerConfig {
    hostname: string;
    password: string;
    passwordAdmin: string;
    enableWhitelist: number;
    disableBanlist: boolean;
}

export default function ServerDefaultEditor() {

    const [serverConfig, setServerConfig] = useState<ServerConfig>();

    useEffect(() => {
        populateServerConfig();
    }, []);

    return (
        <div>
            <TextField id="hostname" variant="outlined" label="Hostname" defaultValue={serverConfig?.hostname} />
            <TextField id="password" variant="outlined" label="Password" defaultValue={serverConfig?.password} />
            <TextField id="passwordAdmin" variant="outlined" label="Admin Password" defaultValue={serverConfig?.passwordAdmin} />
            <TextField id="enableWhitelist" variant="outlined" label="Enable Whitelist" defaultValue={serverConfig?.enableWhitelist} />
            <TextField id="disableBanlist" variant="outlined" label="Disable Ban List" defaultValue={serverConfig?.disableBanlist} />
        </div>
    )

    async function populateServerConfig() {
        const response = await fetch('ServerConfig/GetServerConfig');
        const result = (await response.json()) as ServerConfig;
        setServerConfig(result);
    }
}