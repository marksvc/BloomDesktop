/** @jsx jsx **/
import { jsx, css } from "@emotion/core";

import * as React from "react";
import Button from "@material-ui/core/Button";
import Menu from "@material-ui/core/Menu";
import MenuItem from "@material-ui/core/MenuItem";
import { useL10n } from "../react_components/l10nHooks";
import { Div } from "../react_components/l10nComponents";
import { TextWithEmbeddedLink } from "../react_components/link";
import BloomButton from "../react_components/bloomButton";
import { BloomApi } from "../utils/bloomApi";
import {
    DialogCancelButton,
    WarningBox
} from "../react_components/BloomDialog/commonDialogComponents";
import {
    BloomDialog,
    DialogTitle,
    DialogBottomButtons,
    DialogBottomLeftButtons,
    DialogMiddle
} from "../react_components/BloomDialog/BloomDialog";

// Dialog shown (when props.open is true) in response to the "Forget changes & Check in Book..." menu item
// in the TeamCollectionBookStatusPanel.
export const ForgetChangesDialog: React.FunctionComponent<{
    open: boolean;
    close: () => void;
}> = props => {
    var title = useL10n(
        "Forget Changes & Check in Book",
        "TeamCollection.ForgetChangesDialogTitle",
        undefined,
        undefined,
        undefined,
        true
    );
    return (
        <BloomDialog open={props.open} onClose={props.close}>
            <DialogTitle title={title} />
            <DialogMiddle>
                <Div
                    css={css`
                        margin-bottom: 20px;
                    `}
                    l10nKey="TeamCollection.WorkWillBeLost"
                    temporarilyDisableI18nWarning={true}
                >
                    Any work you have done on this book since you last checked
                    it out will be lost.
                </Div>
                <WarningBox>
                    <Div
                        l10nKey="TeamCollection.CannotUndo" // review: Common.CannotUndo?
                        temporarilyDisableI18nWarning={true}
                    >
                        Warning: You cannot undo this command.
                    </Div>
                </WarningBox>
            </DialogMiddle>
            <DialogBottomButtons>
                <DialogBottomLeftButtons>
                    <BloomButton
                        id="forget"
                        variant="text"
                        enabled={true}
                        l10nKey="TeamCollection.ForgetChanges"
                        temporarilyDisableI18nWarning={true}
                        onClick={() => {
                            props.close();
                            // Do nothing here on either success or failure. (C# code will have already reported failure).
                            BloomApi.post(
                                "teamCollection/forgetChangesInSelectedBook",
                                () => {},
                                () => {}
                            );
                        }}
                        hasText={true}
                    >
                        Forget My Changes
                    </BloomButton>
                </DialogBottomLeftButtons>
                <DialogCancelButton default={true} onClick={props.close} />
            </DialogBottomButtons>
        </BloomDialog>
    );
};
