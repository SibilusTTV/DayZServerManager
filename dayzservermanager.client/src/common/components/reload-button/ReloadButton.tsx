
import { DefaultButton } from '@fluentui/react';
import ReplayIcon from '@mui/icons-material/Replay';

interface ReloadButtonProps {
    populateFunction: Function;
    setFunction: Function;
    endpoint: string;
}

export default function ReloadButton({populateFunction, setFunction, endpoint}: ReloadButtonProps){
    return (
        <DefaultButton
            onClick={() => populateFunction(setFunction, endpoint)}
        >
            <ReplayIcon />
        </DefaultButton>
    )
}