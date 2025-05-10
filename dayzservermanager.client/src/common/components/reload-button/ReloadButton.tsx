
import { DefaultButton } from '@fluentui/react';
import ReplayIcon from '@mui/icons-material/Replay';

interface ReloadButtonProps {
    populateFunction: Function;
    setFunction: Function;
    endpoint?: string;
    setFunctionUnsorted?: Function
}

export default function ReloadButton({ populateFunction, setFunction, endpoint, setFunctionUnsorted }: ReloadButtonProps) {
    if (setFunctionUnsorted == undefined && endpoint != undefined) {
        return (
            <DefaultButton
                onClick={() => populateFunction(setFunction, endpoint)}
            >
                <ReplayIcon />
            </DefaultButton>
        )
    }
    else {
        return (
            <DefaultButton
                onClick={() => populateFunction(setFunction, setFunctionUnsorted)}
            >
                <ReplayIcon />
            </DefaultButton>
        )
    }
}