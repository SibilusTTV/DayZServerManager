
import { DefaultButton } from "@fluentui/react";

interface UnwhitelistPlayerProps {
    guid: string;
    name: string;
    reload: Function;
}

export default function UnwhitelistButton({ guid, name, reload }: UnwhitelistPlayerProps) {
    return (
        <DefaultButton
            onClick={() => WhitelistPlayer(guid, name, reload)}
            className="Button"
        >
            Unwhitelist
        </DefaultButton>
    )
}

async function WhitelistPlayer(_guid: string, _name: string, reload: Function) {
    try {
        await fetch('Scheduler/UnwhitelistPlayer', {
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
    reload();
}