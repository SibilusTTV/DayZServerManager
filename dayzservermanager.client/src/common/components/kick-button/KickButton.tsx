import { DefaultButton } from "@fluentui/react";
import DoNotStepIcon from '@mui/icons-material/DoNotStep';


interface KickButtonProps {
    guid: string;
    name: string;
    reload: Function;
}

export default function KickButton({ guid, name, reload }: KickButtonProps) {

    const KickPlayer = (guid: string, name: string, reload: Function) => {
        let reason = prompt("Please give a reason for the kick");
        if (reason == null) {
            reason = "";
        }
        sendKickRequest(guid, name, reason, reload);
    }

    return (
        <DefaultButton
            onClick={() => KickPlayer(guid, name, reload)}
            style={{ display: "flex", flexDirection: "column" }}
        >
            <DoNotStepIcon />
        </DefaultButton>
    )
}

async function sendKickRequest(_guid: string, _name: string, _reason: string, reload: Function) {
    try {
        await fetch('DayZServer/KickPlayer', {
            method: "POST",
            mode: "cors",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                guid: _guid,
                name: _name,
                reason: _reason
            })
        });
    }
    catch (ex) {
        alert(ex);
    }
    reload();
}