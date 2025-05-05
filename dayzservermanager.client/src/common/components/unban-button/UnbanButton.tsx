import { DefaultButton } from "@fluentui/react";
import CheckIcon from '@mui/icons-material/Check';


interface UnbanPlayerProps {
    guid: string,
    name: string
}

export default function UnbanButton({ guid, name }: UnbanPlayerProps) {
    return (
        <DefaultButton
            onClick={() => sendUnbanRequest(guid, name)}
            style={{ display: "flex", flexDirection: "column" }}
        >
            <CheckIcon />
        </DefaultButton>
    )
}

async function sendUnbanRequest(_guid: string, _name: string) {
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
    }
    catch (ex) {
        alert(ex);
    }
}