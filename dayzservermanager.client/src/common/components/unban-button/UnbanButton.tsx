
import { DefaultButton } from "@fluentui/react";
import CheckIcon from '@mui/icons-material/Check';

interface UnbanPlayerProps {
    guid: string;
    name: string;
    reload: Function;
}

export default function UnbanButton({ guid, name, reload }: UnbanPlayerProps) {
    return (
        <DefaultButton
            onClick={() => sendUnbanRequest(guid, name, reload)}
            style={{ display: "flex", flexDirection: "column" }}
        >
            <CheckIcon />
        </DefaultButton>
    )
}

async function sendUnbanRequest(_guid: string, _name: string, reload: Function) {
    try {
        await fetch('DayZServer/UnbanPlayer', {
            method: "POST",
            mode: "cors",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                guid: _guid,
                name: _name
            })
        });
        alert("Unban sent");
    }
    catch (ex) {
        alert(ex);
    }
    reload();
}