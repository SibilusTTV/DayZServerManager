
import { DefaultButton } from '@fluentui/react';
import SaveIcon from '@mui/icons-material/Save';

interface SaveButtonProps {
    handleSave: Function;
}

export default function SaveButton({ handleSave }: SaveButtonProps) {
    return (
        <DefaultButton
            onClick={() => handleSave()}
            className="Button"
        >
            <SaveIcon/>
        </DefaultButton>
    )
}