
import { DefaultButton } from "@fluentui/react";

interface WhitelistPlayerProps {
    guid: string;
    name: string;
    reload: Function;
}

export default function WhitelistButton({ guid, name, reload }: WhitelistPlayerProps) {
    return (
        <DefaultButton
            onClick={() => WhitelistPlayer(guid, name, reload)}
            className="Button"
        >
            Whitelist
        </DefaultButton>
    );
}

async function WhitelistPlayer(_guid: string, _name: string, reload: Function) {
    try {
        await fetch('Scheduler/WhitelistPlayer', {
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