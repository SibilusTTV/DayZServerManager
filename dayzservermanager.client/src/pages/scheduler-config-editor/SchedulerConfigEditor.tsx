
import { useEffect, useState } from "react";
import SaveButton from "../../common/components/save-button/SaveButton";
import ReloadButton from "../../common/components/reload-button/ReloadButton";
import { TextField } from "@mui/material";


interface SchedulerConfig{
    bannedMessage: string;
    useWhiteList: string;
    whitelistFile: string;
    whitelistKickMsg: string;
    useNickFilter: boolean;
    nickFilterFile: string;
    filteredNickMsg: string;
    timeout: number;
    connectTimeout: number;
}


export default function SchedulerConfigEditor() {
    const [schedulerConfig, setSchedulerConfig] = useState<SchedulerConfig>();

    useEffect(() => {
        PopulateSchedulerManagerConfig(setSchedulerConfig, 'SchedulerConfig/GetSchedulerConfig');
    }, []);

    const handleBlur = (event: React.FocusEvent<HTMLInputElement>) => {
        if (schedulerConfig) {
            setSchedulerConfig(
                {
                    ...schedulerConfig,
                    [event.target.id]: (event.target.value.toLowerCase() == "true") ? true : (event.target.value.toLowerCase() == "false" ? false : event.target.value)
                }
            );
        }
    }

    return (
        <div>
            <h1 id="tableLabel">Scheduler Configurations</h1>
            <div>
                <SaveButton
                    postFunction={PostSchedulerConfig}
                    data={JSON.stringify(schedulerConfig)}
                    endpoint='SchedulerConfig/PostSchedulerConfig'
                />
                <ReloadButton
                    populateFunction={PopulateSchedulerManagerConfig}
                    setFunction={setSchedulerConfig}
                    endpoint='SchedulerConfig/GetSchedulerConfig'
                />
            </div>
            <div>
                {
                    schedulerConfig && Object.entries(schedulerConfig).map(([key, value]) => <TextField key={key} id={key} label={key} onBlur={handleBlur} defaultValue={value} />)
                }
            </div>
        </div>
    )
}

async function PopulateSchedulerManagerConfig(setSchedulerConfig: Function, endpoint: string) {
    const response = await fetch(endpoint);
    const result = (await response.json()) as SchedulerConfig;
    if (result != null) {
        setSchedulerConfig(result);
    }
}

async function PostSchedulerConfig(endpoint: string, json: string) {
    const response = await fetch(endpoint, {
        method: "POST",
        mode: "cors",
        headers: { 'Content-Type': 'application/json' },
        body: json
    });
    const result = await response.text();
    if (result != null) {
        alert(result);
    }
}
