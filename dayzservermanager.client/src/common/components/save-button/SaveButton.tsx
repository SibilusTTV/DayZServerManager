
import { DefaultButton } from '@fluentui/react';
import SaveIcon from '@mui/icons-material/Save';

interface SaveButtonProps {
    postFunction: Function;
    endpoint: string;
    data: string;
}

export default function SaveButton({postFunction, endpoint, data}: SaveButtonProps) {
    return (
        <DefaultButton
            onClick={() => postFunction(endpoint, data) }
        >
            <SaveIcon/>
        </DefaultButton>
    )
}