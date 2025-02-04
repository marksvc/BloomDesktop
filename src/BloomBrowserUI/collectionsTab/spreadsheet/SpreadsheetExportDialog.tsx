/** @jsx jsx **/
import { jsx, css } from "@emotion/core";

import * as React from "react";
import { useL10n } from "../../react_components/l10nHooks";
import BloomButton from "../../react_components/bloomButton";
import { BloomApi } from "../../utils/bloomApi";
import {
    BloomDialog,
    DialogMiddle,
    DialogBottomButtons,
    DialogBottomLeftButtons,
    DialogTitle,
    IBloomDialogProps
} from "../../react_components/BloomDialog/BloomDialog";
import {
    DialogCancelButton,
    DialogFolderChooser,
    WarningBox
} from "../../react_components/BloomDialog/commonDialogComponents";

import { kVerticalSpacingBetweenDialogSections } from "../../bloomMaterialUITheme";
import { ExperimentalBadge } from "../../react_components/experimentalBadge";
import { Div } from "../../react_components/l10nComponents";
import { useEventLaunchedBloomDialog } from "../../react_components/BloomDialog/BloomDialogPlumbing";

export const SpreadsheetExportDialogLauncher: React.FunctionComponent<{}> = () => {
    const {
        openingEvent,
        closeDialog,
        propsForBloomDialog
    } = useEventLaunchedBloomDialog("SpreadsheetExportDialog");

    // We extract the core here so that we can avoid running most of the hook code when this dialog is not visible.
    return propsForBloomDialog.open ? (
        <SpreadsheetExportDialog
            closeDialog={closeDialog}
            propsForBloomDialog={propsForBloomDialog}
            folderPath={openingEvent.folderPath}
        />
    ) : null;
};

const SpreadsheetExportDialog: React.FunctionComponent<{
    closeDialog: () => void;
    propsForBloomDialog: IBloomDialogProps;
    folderPath: string;
}> = props => {
    var title = useL10n(
        "Export to Spreadsheet...",
        "CollectionTab.BookMenu.ExportToSpreadsheet" // same as the menu
    );
    var chooseFolderDescription = useL10n(
        "Target folder for the spreadsheet and images:",
        "Spreadsheet.ExportDialog.folderLabel"
    );
    const [folderPath, setFolderPath] = React.useState(props.folderPath);

    return (
        <BloomDialog {...props.propsForBloomDialog}>
            <DialogTitle title={title}>
                <ExperimentalBadge />
            </DialogTitle>
            <DialogMiddle
                css={css`
                    // our geckofx60 doesn't support this. So there is a <p></p> below instead
                    // gap: ${kVerticalSpacingBetweenDialogSections};
                `}
            >
                <Div l10nKey="Spreadsheet.ExportDialog.Description">
                    Bloom will create a spreadsheet containing the text and
                    images of this book. You can then make changes, like making
                    new translations. Finally, use the “Import &amp; Update from
                    Spreadsheet...” command to bring your changes back in.
                </Div>
                <p></p>
                <WarningBox>
                    <span>
                        This feature is still a work in progress. Though you can{" "}
                        <em>export</em> all books, Bloom cannot <em>import</em>{" "}
                        books with these features: TalkingBooks, Quizzes and
                        other Activities. There may be other export and import
                        limitations we are not aware of.
                    </span>
                </WarningBox>
                <p></p>
                <div>{chooseFolderDescription}</div>

                <DialogFolderChooser
                    path={folderPath}
                    setPath={setFolderPath}
                    description={chooseFolderDescription}
                />
            </DialogMiddle>
            <DialogBottomButtons>
                <DialogBottomLeftButtons>
                    {/* Currently we don't really have any help for this.

                    <HelpLink
                        helpId="Tasks/Basic_tasks/Export_to_Spreadsheet.htm"
                        l10nKey="Common.Help"
                    >
                        Help
                    </HelpLink> */}
                </DialogBottomLeftButtons>
                <BloomButton
                    enabled={true}
                    variant="contained"
                    l10nKey="Spreadsheet.ExportDialog.ExportButton"
                    hasText={true}
                    size="medium"
                    onClick={() => {
                        BloomApi.postData("spreadsheet/export", {
                            parentFolderPath: folderPath
                        });
                        props.closeDialog();
                    }}
                >
                    Export
                </BloomButton>
                <DialogCancelButton onClick={props.closeDialog} />
            </DialogBottomButtons>
        </BloomDialog>
    );
};
