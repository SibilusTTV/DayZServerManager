
import { DefaultButton } from '@fluentui/react';
import ReplayIcon from '@mui/icons-material/Replay';

interface ReloadButtonProps {
    handleLoad: Function;
}

export default function ReloadButton({ handleLoad }: ReloadButtonProps) {
    return (
        <DefaultButton
            onClick={() => handleLoad()}
            className="Button"
        >
            <ReplayIcon />
        </DefaultButton>
    )
}