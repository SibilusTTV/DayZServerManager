
import { DefaultButton } from "@fluentui/react";
import NotInterestedIcon from '@mui/icons-material/NotInterested';


interface BanOfflineButtonProps {
    guid: string;
    name: string;
}

export default function BanOfflineButton({guid, name}: BanOfflineButtonProps) {

    const BanPlayer = (guid: string, name: string) => {
        let reason = prompt("Please give a reason for the ban");
        if (reason == null) {
            reason = "";
        }
        let durationString = prompt("Please give a duration for the ban or leave it empty for a permanent ban");
        let duration = -1;
        if (durationString != null && durationString != "") {
            duration = GetMinutes(durationString);
        }
        sendBanRequest(guid, name, duration, reason);
    }


    const GetMinutes = (durationString: string): number => {
        let pattern = /([0-9]+)[^\S\n]*([Yy]ears?|[Yy]|[Mm]onths?|[Dd]ays?|[Dd]|[Hh]ours?|[Hh]|[Mm]inutes?|[Mm]ins?|[Mm]|)/gm
        let matches = durationString.matchAll(pattern);
        let finalvalue = 0;
        for (let match of matches) {
            let number = parseInt(match[1].valueOf());
            let type = match[2].valueOf();
            switch (type) {
                case "Year":
                case "Years":
                case "year":
                case "years":
                case "Y":
                case "y":
                    finalvalue += number * 365 * 24 * 60;
                    break;
                case "Month":
                case "Months":
                case "month":
                case "months":
                    finalvalue += number * 30 * 24 * 60;
                    break;
                case "Day":
                case "Days":
                case "day":
                case "days":
                case "D":
                case "d":
                    finalvalue += number * 24 * 60;
                    break;
                case "Hour":
                case "Hours":
                case "hour":
                case "hours":
                case "H":
                case "h":
                    finalvalue += number * 60;
                    break;
                case "Minute":
                case "Minutes":
                case "minute":
                case "minutes":
                case "Min":
                case "Mins":
                case "min":
                case "mins":
                case "M":
                case "m":
                default:
                    finalvalue += number;
                    break;

            }
        }
        return finalvalue;
    }

    return (
        <DefaultButton
            onClick={() => BanPlayer(guid, name)}
            style={{ display: "flex", flexDirection: "column" }}
        >
            <NotInterestedIcon />
        </DefaultButton>
    )

}

async function sendBanRequest(_guid: string, _name: string, _duration: number, _reason: string) {
    try {
        await fetch('DayZServer/BanPlayer', {
            method: "POST",
            mode: "cors",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                guid: _guid,
                name: _name,
                duration: _duration,
                reason: _reason
            })
        });
    }
    catch (ex) {
        alert(ex);
    }
}