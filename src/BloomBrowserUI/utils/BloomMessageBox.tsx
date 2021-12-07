/** @jsx jsx **/
import { jsx, css } from "@emotion/core";

import * as React from "react";
import { WireUpForWinforms } from "../utils/WireUpWinform";
import { DialogContent, DialogContentText } from "@material-ui/core";
import BloomButton from "../react_components/bloomButton";
import { BloomApi } from "./bloomApi";
import WarningOutlinedIcon from "@material-ui/icons/WarningOutlined";
import {
    BloomDialog,
    IBloomDialogEnvironmentParams,
    useSetupBloomDialog,
    DialogBottomButtons
} from "../react_components/BloomDialog/BloomDialog";
import { useEffect } from "react";

export interface MessageBoxButton {
    text: string;
    id: string;
    default: boolean; // Only one button should have this true
}

// Designed to be a partial replacement for a WinForms messageBox, both from C# and Typescript (eventually...needs work).
// More flexible in that buttons can be fully configured, and uses our MaterialUI dialog look and feel.
export const BloomMessageBox: React.FunctionComponent<{
    messageHtml: string; // The localized message to notify the user about. Can contain HTML.
    rightButtons: MessageBoxButton[];
    icon?: "warning" | undefined; // Effectively an enumeration, which we will add to as needed
    dialogEnvironment?: IBloomDialogEnvironmentParams;
    // For use from Typescript, provide a callback to invoke when a button is clicked?
    // Probably also need a way to control whether it is open.
    // And maybe turn off the BloomApi behavior.
    // callback? : (messageId: string, buttonId:string) => void
}> = props => {
    const {
        showDialog,
        closeDialog,
        propsForBloomDialog
    } = useSetupBloomDialog(props.dialogEnvironment);

    const closeDialogForButton = buttonId => {
        // Enhance: do something else if called from Typescript. Close the dialog and somehow
        // report what was clicked.
        BloomApi.postString("common/closeReactDialog", buttonId);
    };
    const rightButtons = props.rightButtons.map(button => (
        <BloomButton
            className={button.default ? "initialFocus" : ""}
            key={button.id}
            enabled={true}
            l10nKey=""
            alreadyLocalized={true}
            hasText={true}
            variant={button.default ? "contained" : "outlined"}
            onClick={() => closeDialogForButton(button.id)}
        >
            {button.text}
        </BloomButton>
    ));

    return (
        <BloomDialog {...propsForBloomDialog}>
            <DialogContent>
                <div
                    id="root"
                    css={css`
                        display: flex;
                    `}
                >
                    {/* InnerHTML is used so that we can insert markup like <br> into the message. */}

                    {props.icon === "warning" && (
                        <WarningOutlinedIcon
                            css={css`
                                font-size: 3rem !important;
                                color: orange;
                            `}
                        />
                    )}
                    <DialogContentText
                        className="allowSelect"
                        dangerouslySetInnerHTML={{
                            __html: props.messageHtml || ""
                        }}
                    />
                </div>
            </DialogContent>
            <DialogBottomButtons>{rightButtons}</DialogBottomButtons>
        </BloomDialog>
    );
};

WireUpForWinforms(BloomMessageBox);